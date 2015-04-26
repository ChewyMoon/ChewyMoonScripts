using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace OneKeyToFish
{
    internal class Program
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static Menu Menu { get; set; }
        public static Orbwalking.Orbwalker Orbwalker { get; set; }
        private static Vector3? LastHarassPos { get; set; }
        private static Obj_AI_Hero DrawTarget { get; set; }
        private static Geometry.Polygon.Rectangle RRectangle { get; set; }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        #region OneKeyToFish :: Menu

        private static void CreateMenu()
        {
            Menu = new Menu("OneKeyToFish", "cmFizzKAPPA", true);

            // Orbwalker
            var orbwalkMenu = new Menu("Orbwalking", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "ts");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Combo
            var comboMenu = new Menu("Combo", "combo");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseREGapclose", "Use R, then E for gapclose if killable").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "harass");
            harassMenu.AddItem(new MenuItem("UseQMixed", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseWMixed", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseEMixed", "Use E").SetValue(true));
            harassMenu.AddItem(
                new MenuItem("UseEHarassMode", "E Mode: ").SetValue(
                    new StringList(new[] { "Back to Position", "On Enemy" })));
            Menu.AddSubMenu(harassMenu);

            // Misc
            var miscMenu = new Menu("Misc", "miscerino");
            miscMenu.AddItem(
                new MenuItem("UseWWhen", "Use W: ").SetValue(new StringList(new[] { "Before Q", "After Q" })));
            miscMenu.AddItem(new MenuItem("UseETower", "Dodge tower shots with E").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "draw");
            drawMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawRPred", "Draw R Prediction").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            Menu.AddToMainMenu();
        }

        #endregion OneKeyToFish :: Menu

        #region Spells

        private static Spell Q { get; set; }
        private static Spell W { get; set; }
        private static Spell E { get; set; }
        private static Spell R { get; set; }

        #endregion Spells

        #region GameLoad

        private static void GameOnOnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != "Fizz")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(Player));
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 1300);

            E.SetSkillshot(0.25f, 330, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 80, 1200, false, SkillshotType.SkillshotLine);

            CreateMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = DamageToUnit;
            Utility.HpBarDamageIndicator.Enabled = true;

            RRectangle = new Geometry.Polygon.Rectangle(Player.Position, Player.Position, R.Width);

            Game.OnUpdate += GameOnOnUpdate;
            Obj_AI_Base.OnProcessSpellCast += ObjAiBaseOnOnProcessSpellCast;
            Drawing.OnDraw += DrawingOnOnDraw;

            Game.PrintChat("<font color=\"#7CFC00\"><b>OneKeyToFish:</b></font> Loaded");
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("DrawQ").IsActive();
            var drawE = Menu.Item("DrawE").IsActive();
            var drawR = Menu.Item("DrawR").IsActive();
            var drawRPred = Menu.Item("DrawRPred").IsActive();
            var p = Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawRPred && R.IsReady() && DrawTarget.IsValidTarget())
            {
                RRectangle.Draw(Color.CornflowerBlue, 3);
            }
        }


        private static float DamageToUnit(Obj_AI_Hero target)
        {
            var damage = 0d;

            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (W.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.R);
            }

            return (float) damage;
        }

        private static void ObjAiBaseOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender is Obj_AI_Turret && args.Target.IsMe && E.IsReady() && Menu.Item("UseETower").IsActive())
            {
                E.Cast(Game.CursorPos);
            }

            if (!sender.IsMe)
            {
                return;
            }

            if (args.SData.Name == "FizzPiercingStrike")
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    Utility.DelayAction.Add((int) (sender.Spellbook.CastEndTime - Game.Time) + Game.Ping / 2 + 250, () => W.Cast());
                }
                else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed &&
                         Menu.Item("UseEHarassMode").GetValue<StringList>().SelectedIndex == 0)
                {
                    Utility.DelayAction.Add(
                        (int)(sender.Spellbook.CastEndTime - Game.Time) + Game.Ping / 2 + 250, () => { JumpBack = true; });
                }
            }

            if (args.SData.Name == "fizzjumptwo" || args.SData.Name == "fizzjumpbuffer")
            {
                LastHarassPos = null;
                JumpBack = false;
            }
        }

        public static bool JumpBack { get; set; }

        #endregion GameLoad

        #region Update

        private static void GameOnOnUpdate(EventArgs args)
        {
            DrawTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (DrawTarget.IsValidTarget())
            {
                RRectangle.Start = Player.Position.To2D();
                RRectangle.End = R.GetPrediction(DrawTarget).CastPosition.To2D();
                RRectangle.UpdatePolygon();
            }

            if (!Player.CanCast)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }
        }

        public static void CastRSmart(Obj_AI_Hero target)
        {
            var castPosition = R.GetPrediction(target).CastPosition;
            castPosition = Player.ServerPosition.Extend(castPosition, R.Range);

            R.Cast(castPosition);
        }

        private static void DoCombo()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Menu.Item("UseREGapclose").IsActive() && CanKillWithUltCombo(target) && Q.IsReady() && W.IsReady() &&
                E.IsReady() && R.IsReady() && (Player.Distance(target) < Q.Range + E.Range * 2))
            {
                CastRSmart(target);

                E.Cast(Player.ServerPosition.Extend(target.ServerPosition, E.Range - 1));
                E.Cast(Player.ServerPosition.Extend(target.ServerPosition, E.Range - 1));

                W.Cast();
                Q.Cast(target);
            }
            else
            {
                if (R.IsEnabledAndReady())
                {
                    if (Player.GetSpellDamage(target, SpellSlot.R) > target.Health)
                    {
                        CastRSmart(target);
                    }

                    if (DamageToUnit(target) > target.Health)
                    {
                        CastRSmart(target);
                    }

                    if ((Q.IsReady() || E.IsReady()))
                    {
                        CastRSmart(target);
                    }

                    if (Orbwalker.InAutoAttackRange(target))
                    {
                        CastRSmart(target);
                    }
                }

                // Use W Before Q
                if (W.IsEnabledAndReady() && Menu.Item("UseWWhen").GetValue<StringList>().SelectedIndex == 0 &&
                    (Q.IsReady() || Orbwalker.InAutoAttackRange(target)))
                {
                    W.Cast();
                }

                if (Q.IsEnabledAndReady())
                {
                    Q.Cast(target);
                }

                if (E.IsEnabledAndReady())
                {
                    E.Cast(target);
                }
            }
        }

        public static bool CanKillWithUltCombo(Obj_AI_Hero target)
        {
            return Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.W) + Player.GetSpellDamage(target, SpellSlot.R) >
                   target.Health;
        }

        private static void DoHarass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (LastHarassPos == null)
            {
                LastHarassPos = ObjectManager.Player.ServerPosition;
            }

            if (JumpBack)
            {
                E.Cast((Vector3) LastHarassPos);
            }

            // Use W Before Q
            if (W.IsEnabledAndReady() && Menu.Item("UseWWhen").GetValue<StringList>().SelectedIndex == 0 &&
                (Q.IsReady() || Orbwalker.InAutoAttackRange(target)))
            {
                W.Cast();
            }

            if (Q.IsEnabledAndReady())
            {
                Q.Cast(target);
            }

            if (E.IsEnabledAndReady() && Menu.Item("UseEHarassMode").GetValue<StringList>().SelectedIndex == 1)
            {
                E.Cast(target);
            }
        }

        #endregion Update
    }

    internal static class SpellEx
    {
        public static bool IsEnabledAndReady(this Spell spell)
        {
            return Program.Menu.Item("Use" + spell.Slot + Program.Orbwalker.ActiveMode).IsActive() && spell.IsReady();
        }
    }
}