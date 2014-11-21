#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Mid_or_Feed.Champions
{
    internal class Lux : Plugin
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static GameObject EGameObject;

        public Lux()
        {
            Q = new Spell(SpellSlot.Q, 1300);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            Q.SetSkillshot(0.5f, 80, 1200, false, SkillshotType.SkillshotLine);
                // no collision, manual check for collision with minion
            W.SetSkillshot(0.5f, 150, 1200, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 275, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.75f, 190, 3000, false, SkillshotType.SkillshotLine);

            GameObject.OnCreate += delegate(GameObject sender, EventArgs args)
            {
                if (sender.Name.Contains("LuxLightstrike_tar_green"))
                    EGameObject = sender;
            };

            GameObject.OnDelete += delegate(GameObject sender, EventArgs args)
            {
                if (sender.Name.Contains("LuxLightstrike_tar_green"))
                    EGameObject = null;
            };

            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            Game.OnGameProcessPacket += GameOnOnGameProcessPacket;
            AntiGapcloser.OnEnemyGapcloser +=AntiGapcloserOnOnEnemyGapcloser;
        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!GetBool("qGapcloser"))
                return;

            Q.Cast(gapcloser.Sender, Packets);
        }

        private void GameOnOnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!GetBool("rKSRecall"))
                return;

            if (args.PacketData[0] != Packet.S2C.Recall.Header) return;
            Console.WriteLine("Got recall packet");
            var decoded = Packet.S2C.Recall.Decoded(args.PacketData);
            var target = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(decoded.UnitNetworkId);

            if(target.IsAlly)
                return;

            if (decoded.Status != Packet.S2C.Recall.RecallStatus.RecallStarted) return;

            var rdmg = Player.GetDamageSpell(target, SpellSlot.R).CalculatedDamage;
            Console.WriteLine("Target: {0} | R DMG: {1} | ENEMY HEALTH: {2}", target.ChampionName, rdmg, target.Health);
            if (rdmg > target.Health)
                R.Cast(target, Packets);
        }

        public static bool EActivated
        {
            get { return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1 || EGameObject != null; }
        }

        public static bool HasPassive(Obj_AI_Hero hero)
        {
            return hero.HasBuff("luxilluminatingfraulein");
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
            switch (OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }

            if (GetBool("autoW"))
                AutoW();

            if (GetBool("rKS"))
                ItsKillSecure();
        }

        private void ItsKillSecure()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget()).Where(enemy => Player.GetDamageSpell(enemy, SpellSlot.R).CalculatedDamage > enemy.Health))
            {
                R.Cast(enemy, Packets);
                return;
            }
        }

        private void AutoW()
        {
            foreach (var ally in from ally in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly).Where(x => !x.IsDead) let allyPercent = ally.Health/ally.MaxHealth*100 let healthPercent = GetValue<Slider>("autoWPercent").Value where healthPercent >= allyPercent select ally)
            {
                W.Cast(ally, Packets);
                return;
            }
        }

        private void CastQ(Obj_AI_Base target)
        {
            var input = Q.GetPrediction(target);
            var col = Q.GetCollision(Player.ServerPosition.To2D(), new List<Vector2> {input.CastPosition.To2D()});
            var minions = col.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

            if (minions <= 1)
                Q.Cast(input.CastPosition, Packets);
        }

        private void CastE(Obj_AI_Hero target)
        {
            if (EActivated)
            {
                if (
                    !ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsEnemy)
                        .Where(x => !x.IsDead)
                        .Any(enemy => enemy.Distance(EGameObject.Position) <= E.Width)) return;

                var isInAaRange = Player.Distance(target) <= Orbwalking.GetRealAutoAttackRange(Player);

                if(isInAaRange && !HasPassive(target))
                    E.Cast(Player, Packets);

                // Pop E if the target is out of AA range
                if (!isInAaRange)
                    E.Cast(Player, Packets);
            }
            else
            {
                E.Cast(target, Packets);
            }
        }

        private void DoCombo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (!target.IsValidTarget())
                return;

            var useQ = GetBool("useQ");
            var useW = GetBool("useW");
            var useE = GetBool("useE");
            var useR = GetBool("useR");
            var useRKillable = GetBool("useRKillable");

            if (useQ && !HasPassive(target))
            {
                CastQ(target);
            }

            if (useW)
            {
                W.Cast(Game.CursorPos, Packets);
            }

            if (useE)
            {
                CastE(target);
            }

            if (useR)
            {
                R.Cast(target, Packets);
            }

            if (!useRKillable) return;
            var killable = Player.GetDamageSpell(target, SpellSlot.R).CalculatedDamage > target.Health;
            if (killable)
                R.Cast(target);
        }

        private void DoHarass()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (!target.IsValidTarget())
                return;

            var useQ = GetBool("useQHarass");
            var useE = GetBool("useEHarass");

            if (useQ && !HasPassive(target))
            {
                CastQ(target);
            }

            if (!useE) return;
            CastE(target);
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("drawQ");
            var drawW = GetBool("drawW");
            var drawE = GetBool("drawE");
            var drawR = GetBool("drawR");
            var drawRMinimap = GetBool("drawRMinimap");

            var p = Player.Position;

            if (drawQ)
            {
                Utility.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Utility.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Utility.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Utility.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawRMinimap)
            {
                Utility.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red, 2, 30, true);
            }
        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;
            var p = Player;

            if (Q.IsReady())
               dmg += p.GetDamageSpell(target, SpellSlot.Q).CalculatedDamage;

            if (E.IsReady())
                dmg += p.GetDamageSpell(target, SpellSlot.E).CalculatedDamage;

            if (R.IsReady())
                dmg += p.GetDamageSpell(target, SpellSlot.R).CalculatedDamage;

            return (float) dmg;

        }

        public override void Combo(Menu comboMenu)
        {
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(false));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(false));
            comboMenu.AddItem(new MenuItem("useRKillable", "R only if Killable").SetValue(true));

            comboMenu.Item("useRKillable").ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
            {
                if (!GetBool("useR"))
                    return;

                var value = args.GetNewValue<bool>();
                Menu.Item("useR").SetValue(!value);
            };

            comboMenu.Item("useR").ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
            {
                if (!GetBool("useRKillable")) return;
                var value = args.GetNewValue<bool>();
                Menu.Item("useRKillable").SetValue(!value);
            };
        }

        public override void Harass(Menu harassMenu)
        {
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));
        }

        public override void Items(Menu itemsMenu)
        {
            //itemsMenu.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
        }

        public override void Misc(Menu miscMenu)
        {
            miscMenu.AddItem(new MenuItem("autoW", "Auto use W").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoWPercent", "% Health").SetValue(new Slider()));
            //miscMenu.AddItem(new MenuItem("seperator", " "));
            miscMenu.AddItem(new MenuItem("rKS", "Use R to KS").SetValue(true));
            miscMenu.AddItem(new MenuItem("rKSRecall", "KS enemies b'ing in FOW").SetValue(true)); // Might be patched..
           // miscMenu.AddItem(new MenuItem("seperator", " "));
            miscMenu.AddItem(new MenuItem("qGapcloser", "Q on Gapcloser").SetValue(true));
        }

        public override void Drawings(Menu drawingMenu)
        {
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawRMinimap", "Draw R(Minimap)").SetValue(true));
        }
    }
}