#region

using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;
using System;
using System.Linq;
using Color = System.Drawing.Color;

#endregion

namespace ChewyMoonsShaco
{
    internal class ChewyMoonShaco
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;

        public static Menu Menu;
        public static LXOrbwalker Orbwalker;

        public static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Shaco") return;

            Q = new Spell(SpellSlot.Q, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRadius[0]);
            W = new Spell(SpellSlot.W, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRadius[0]);
            E = new Spell(SpellSlot.E, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRadius[0]);

            CreateMenu();

            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void CreateMenu()
        {
            (Menu = new Menu("[Chewy's Shaco]", "cmShaco", true)).AddToMainMenu();

            // Target Selector
            var tsMenu = new Menu("Target Selector", "cmShacoTS");
            SimpleTs.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalking
            var orbwalkingMenu = new Menu("Orbwalking", "cmShacoOrbwalkin");
            Orbwalker = new LXOrbwalker();
            LXOrbwalker.AddToMenu(orbwalkingMenu);
            Menu.AddSubMenu(orbwalkingMenu);

            // Combo
            var comboMenu = new Menu("Combo", "cmShacoCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "cmShacoHarass");
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // Drawing
            var drawingMenu = new Menu("Drawings", "cmShacoDrawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(new Circle(true, Color.Khaki)));
            drawingMenu.AddItem(new MenuItem("drawQPos", "Draw Q Pos").SetValue(new Circle(true, Color.Magenta)));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(new Circle(true, Color.Khaki)));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(new Circle(true, Color.Khaki)));
            Menu.AddSubMenu(drawingMenu);

            // Misc
            var miscMenu = new Menu("Misc", "cmShacoMisc");
            miscMenu.AddItem(new MenuItem("stuff", "Let me know of any"));
            miscMenu.AddItem(new MenuItem("stuff2", "other misc features you want"));
            miscMenu.AddItem(new MenuItem("stuff3", "on the thead or IRC"));
            Menu.AddSubMenu(miscMenu);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var qCircle = Menu.Item("drawQ").GetValue<Circle>();
            var wCircle = Menu.Item("drawW").GetValue<Circle>();
            var eCircle = Menu.Item("drawE").GetValue<Circle>();
            var qPosCircle = Menu.Item("drawQPos").GetValue<Circle>();

            var pos = ObjectManager.Player.Position;

            if (qCircle.Active)
            {
                Utility.DrawCircle(pos, Q.Range, qCircle.Color);
            }

            if (wCircle.Active)
            {
                Utility.DrawCircle(pos, W.Range, wCircle.Color);
            }

            if (eCircle.Active)
            {
                Utility.DrawCircle(pos, E.Range, eCircle.Color);
            }

            if (qPosCircle.Active)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget()))
                {
                    var qPos = enemy.Position +
                               Vector3.Normalize(ObjectManager.Player.Position + enemy.Position) * Q.Range;
                    Drawing.DrawLine(Drawing.WorldToScreen(enemy.Position), Drawing.WorldToScreen(qPos), 2, qPosCircle.Color);
                }
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    Combo();
                    break;

                case LXOrbwalker.Mode.Harass:
                    Harass();
                    break;
            }
        }

        private static void Combo()
        {
        }

        private static void Harass()
        {
        }
    }
}