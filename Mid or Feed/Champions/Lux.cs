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
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            Q.SetSkillshot(0.25f, 80, 1200, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 150, 1200, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 275, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.75f, 190, 3000, false, SkillshotType.SkillshotLine);

            GameObject.OnCreate += delegate(GameObject sender, EventArgs args)
            {
                //Noticed a different E game object name while looking through Kurisu's oracle, credits to him.
                if (sender.Name.Contains("LuxLightstrike_tar_green") || sender.Name.Contains("LuxLightstrike_tar_red"))
                {
                    EGameObject = sender;
                }
            };

            GameObject.OnDelete += delegate(GameObject sender, EventArgs args)
            {
                //Noticed a different E game object name while looking through Kurisu's oracle, credits to him.
                if (sender.Name.Contains("LuxLightstrike_tar_"))
                {
                    EGameObject = null;
                }
            };

            Game.OnUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            Obj_AI_Base.OnTeleport += ObjAiHeroOnOnTeleport;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;

            PrintChat("Lux loaded!");
        }

        public static bool EActivated
        {
            get { return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1 || EGameObject != null; }
        }

        private void ObjAiHeroOnOnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            if (!GetBool("rKSRecall"))
            {
                return;
            }

            var decoded = Packet.S2C.Teleport.Decoded(sender, args);
            var hero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(decoded.UnitNetworkId);

            if (hero.IsAlly || decoded.Type != Packet.S2C.Teleport.Type.Recall ||
                decoded.Status != Packet.S2C.Teleport.Status.Start)
            {
                return;
            }

            var rDamage = Player.GetSpellDamage(hero, SpellSlot.R);
            if (rDamage > hero.Health)
            {
                R.Cast(hero);
            }
        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!GetBool("qGapcloser"))
            {
                return;
            }

            Q.Cast(gapcloser.Sender, Packets);
        }

        public static bool HasPassive(Obj_AI_Hero hero)
        {
            return hero.HasBuff("luxilluminatingfraulein", true);
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
            {
                AutoW();
            }

            if (GetBool("rKS"))
            {
                ItsKillSecure();
            }

            // Steal buffs, become challenger ggez
            if (GetValue<KeyBind>("AutoSteal").Active || GetValue<KeyBind>("KeySteal").Active)
            {
                StealBlue(GetBool("StealAllyBlue"), GetBool("StealEnemyBlue"));
            }

            if (GetValue<KeyBind>("AutoSteal").Active || GetValue<KeyBind>("KeySteal").Active)
            {
                StealRed(GetBool("StealAllyRed"), GetBool("StealEnemyRed"));
            }
        }

        private void StealBlue(bool stealFriendly, bool stealEnemy)
        {
            if (!R.IsReady())
            {
                return;
            }

            var blueBuff =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(x => x.BaseSkinName == "SRU_Blue")
                    .Where(x => Player.GetSpellDamage(x, SpellSlot.R) > x.Health)
                    .FirstOrDefault(x => (stealFriendly && x.IsAlly) || (stealEnemy && x.IsEnemy));

            if (blueBuff != null)
            {
                R.Cast(blueBuff, Packets);
            }
        }

        private void StealRed(bool stealFriendly, bool stealEnemy)
        {
            if (!R.IsReady())
            {
                return;
            }

            var redBuff =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(x => x.BaseSkinName == "SRU_Red")
                    .Where(x => Player.GetSpellDamage(x, SpellSlot.R) > x.Health)
                    .FirstOrDefault(x => (stealFriendly && x.IsAlly) || (stealEnemy && x.IsEnemy));

            if (redBuff != null)
            {
                R.Cast(redBuff, Packets);
            }
        }

        private void ItsKillSecure()
        {
            if (!R.IsReady())
            {
                return;
            }

            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget())
                    .Where(x => !x.IsZombie)
                    .Where(x => !x.IsDead)
                    .Where(enemy => Player.GetDamageSpell(enemy, SpellSlot.R).CalculatedDamage > enemy.Health))
            {
                R.Cast(enemy, Packets);
                return;
            }
        }

        private void AutoW()
        {
            if (!W.IsReady() || Player.IsRecalling())
            {
                return;
            }

            foreach (
                var ally in from ally in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly).Where(x => !x.IsDead)
                    let allyPercent = ally.Health / ally.MaxHealth * 100
                    let healthPercent = GetValue<Slider>("autoWPercent").Value
                    where healthPercent >= allyPercent
                    select ally)
            {
                W.Cast(ally, Packets, true);
                return;
            }
        }

        private void CastQ(Obj_AI_Base target)
        {
            var input = Q.GetPrediction(target);
            var col = Q.GetCollision(Player.ServerPosition.To2D(), new List<Vector2> { input.CastPosition.To2D() });
            var minions = col.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

            if (minions <= 1)
            {
                Q.Cast(input.CastPosition, Packets);
            }
        }

        private void CastE(Obj_AI_Hero target)
        {
            //TODO: Remake this
            if (EActivated)
            {
                if (
                    !ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsEnemy)
                        .Where(x => !x.IsDead)
                        .Where(x => x.IsValidTarget())
                        .Any(enemy => enemy.Distance(EGameObject.Position) < E.Width))
                {
                    return;
                }

                var isInAaRange = Player.Distance(target) <= Orbwalking.GetRealAutoAttackRange(Player);

                if (isInAaRange && !HasPassive(target))
                {
                    E.Cast(Packets);
                }

                // Pop E if the target is out of AA range
                if (!isInAaRange)
                {
                    E.Cast(Packets);
                }
            }
            else
            {
                E.Cast(target, Packets);
            }
        }

        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            var useQ = GetBool("useQ");
            var useQSlowed = GetBool("useQSlowed");
            var useW = GetBool("useW");
            var useE = GetBool("useE");
            var useR = GetBool("useR");
            var useRKillable = GetBool("useRKillable");

            if (useQ && !HasPassive(target) && Q.IsReady() && !useQSlowed)
            {
                CastQ(target);
            }

            if (useQSlowed)
            {
                if (target.HasBuffOfType(BuffType.Slow) && EActivated && target.Distance(EGameObject.Position) < E.Width)
                {
                    Q.Cast(target, Packets);
                }

                // Cast Q if E is not up
                if (!E.IsReady() && !EActivated && !HasPassive(target))
                {
                    Q.Cast(target, Packets);
                }
            }

            if (useW && W.IsReady())
            {
                W.Cast(Game.CursorPos, Packets);
            }

            if (useE && E.IsReady())
            {
                CastE(target);
            }

            if (useR && R.IsReady())
            {
                R.Cast(target, Packets, true);
            }

            if (!useRKillable)
            {
                return;
            }
            var killable = Player.GetSpellDamage(target, SpellSlot.R) > target.Health;
            if (killable && R.IsReady())
            {
                R.Cast(target, Packets, true);
            }
        }

        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            var useQ = GetBool("useQHarass");
            var useE = GetBool("useEHarass");

            if (useQ && !HasPassive(target) && Q.IsReady())
            {
                CastQ(target);
            }

            if (!useE || !E.IsReady())
            {
                return;
            }
            CastE(target);
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("drawQ");
            var drawW = GetBool("drawW");
            var drawE = GetBool("drawE");
            var drawR = GetBool("drawR");

            var p = Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;
            var p = Player;

            if (Q.IsReady())
            {
                dmg += p.GetDamageSpell(target, SpellSlot.Q).CalculatedDamage;
            }

            if (E.IsReady())
            {
                dmg += p.GetDamageSpell(target, SpellSlot.E).CalculatedDamage;
            }

            if (R.IsReady())
            {
                dmg += p.GetDamageSpell(target, SpellSlot.R).CalculatedDamage;
            }

            return (float) dmg;
        }

        public override void Combo(Menu comboMenu)
        {
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useQSlowed", "Only use Q if slowed by E").SetValue(false));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(false));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(false));
            comboMenu.AddItem(new MenuItem("useRKillable", "R only if Killable").SetValue(true));

            comboMenu.Item("useRKillable").ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
            {
                if (args.GetNewValue<bool>() && GetBool("useR"))
                {
                    Menu.Item("useR").SetValue(false);
                }
            };

            comboMenu.Item("useR").ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
            {
                if (args.GetNewValue<bool>() && GetBool("useRKillable"))
                {
                    Menu.Item("useRKillable").SetValue(false);
                }
            };
        }

        public override void Harass(Menu harassMenu)
        {
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));
        }

        public override void Misc(Menu miscMenu)
        {
            // Buff Steal Menui
            var buffStealMenu = new Menu("Buff Stealing", "mofLuxBFF");
            buffStealMenu.AddItem(new MenuItem("StealEnemyRed", "Steal Enemy Red Buff").SetValue(true));
            buffStealMenu.AddItem(new MenuItem("StealEnemyBlue", "Steal Enemy Blue").SetValue(true));
            buffStealMenu.AddItem(new MenuItem("StealAllyBlue", "Steal Ally Blue Buff").SetValue(false));
            buffStealMenu.AddItem(new MenuItem("StealAllyRed", "Steal Ally Red Buff").SetValue(false));
            buffStealMenu.AddItem(
                new MenuItem("AutoSteal", "Steal(Toggle)").SetValue(new KeyBind(84, KeyBindType.Press, true)));
            buffStealMenu.AddItem(new MenuItem("KeySteal", "Steal(Press)").SetValue(new KeyBind(90, KeyBindType.Press)));
            miscMenu.AddSubMenu(buffStealMenu);

            miscMenu.AddItem(new MenuItem("rKS", "Use R to KS").SetValue(true));
            miscMenu.AddItem(new MenuItem("rKSRecall", "KS enemies b'ing in FOW").SetValue(true));
            miscMenu.AddItem(new MenuItem("qGapcloser", "Q on Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoW", "Auto use W").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoWPercent", "% Health").SetValue(new Slider(15, 1)));
        }

        public override void Drawings(Menu drawingMenu)
        {
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));
        }
    }
}