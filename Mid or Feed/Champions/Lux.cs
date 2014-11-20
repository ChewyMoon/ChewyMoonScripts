#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

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
        }

        public static bool EActivated
        {
            get { return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1; }
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
                    ;
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

        private void CastE(Obj_AI_Base target)
        {
            if (EActivated)
            {
                if (
                    !ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsEnemy)
                        .Where(x => !x.IsDead)
                        .Any(enemy => enemy.Distance(EGameObject.Position) <= E.Range)) return;
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
            //throw new NotImplementedException();
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
                if (!GetBool("useRKillable"))
                    return;

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
            itemsMenu.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
        }

        public override void Misc(Menu miscMenu)
        {
            miscMenu.AddItem(new MenuItem("autoW", "Auto use W").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoWPercent", "% Health").SetValue(new Slider()));
            miscMenu.AddItem(new MenuItem("seperator", " "));
            miscMenu.AddItem(new MenuItem("rKS", "Use R to KS").SetValue(true));
            miscMenu.AddItem(new MenuItem("rKSRecall", "KS enemies b'ing in FOW").SetValue(true)); // Might be patched..
            miscMenu.AddItem(new MenuItem("seperator", " "));
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