#region

using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace ChewyMoonsLux
{
    internal class ChewyMoonsLux
    {
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static bool PacketCast = false;

        public static bool Debug
        {
            get { return Menu.Item("debug").GetValue<bool>(); }
        }

        public static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Lux")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            // Refine skillshots
            Q.SetSkillshot(0.25f, 80f, 1200f, false, SkillshotType.SkillshotLine); // to get collision objects
            W.SetSkillshot(0.25f, 150f, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 275f, 1300f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.35f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);

            // Setup Main Menu
            SetupMenu();

            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += QGapCloser.OnEnemyGapCloser;
            Game.OnGameUpdate += LuxCombo.OnGameUpdate;
            GameObject.OnCreate += LuxCombo.OnGameObjectCreate;
            GameObject.OnDelete += LuxCombo.OnGameObjectDelete;

            Utilities.PrintChat("Loaded.");
        }

        private static void OnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("drawQ").GetValue<bool>();
            var drawW = Menu.Item("drawW").GetValue<bool>();
            var drawE = Menu.Item("drawE").GetValue<bool>();
            var drawR = Menu.Item("drawR").GetValue<bool>();

            var qColor = Menu.Item("qColor").GetValue<Circle>().Color;
            var wColor = Menu.Item("wColor").GetValue<Circle>().Color;
            var eColor = Menu.Item("eColor").GetValue<Circle>().Color;
            var rColor = Menu.Item("rColor").GetValue<Circle>().Color;

            var position = ObjectManager.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(position, Q.Range, qColor);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(position, W.Range, wColor);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(position, E.Range, eColor);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(position, R.Range, rColor);
            }
        }

        private static void SetupMenu()
        {
            Menu = new Menu("[Chewy's Lux]", "cmLux", true);

            // Target Selector
            var tsMenu = new Menu("[Chewy's Lux] - TS", "cmLuxTs");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkerMenu = new Menu("[Chewy's Lux] - Orbwalker", "cmLuxOrbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            // Combo settings
            var comboMenu = new Menu("[Chewy's Lux] - Combo", "cmLuxCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(false));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("onlyRIfKill", "Use R to kill only").SetValue(false));
            comboMenu.AddItem(new MenuItem("useIgnite", "Use ignite in combo").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass Settings
            var harassMenu = new Menu("[Chewy's Lux] - Harass", "cmLuxHarass");
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // KS / Finisher Settings
            var ksMenu = new Menu("[Chewy's Lux] - KS", "cmLuxKS");
            ksMenu.AddItem(new MenuItem("ultKS", "KS with R").SetValue(true));
            //ksMenu.AddItem(new MenuItem("recallExploitKS", "KS enemies recalling").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            // Items
            var itemsMenu = new Menu("[Chewy's Lux] - Items", "cmLuxItems");
            itemsMenu.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
            Menu.AddSubMenu(itemsMenu);

            //Drawing
            var drawingMenu = new Menu("[Chewy's Lux] - Drawing", "cmLuxDrawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));
            drawingMenu.AddItem(new MenuItem("qColor", "Q Color").SetValue(new Circle(true, Color.Gray)));
            drawingMenu.AddItem(new MenuItem("wColor", "W Color").SetValue(new Circle(true, Color.Gray)));
            drawingMenu.AddItem(new MenuItem("eColor", "E Color").SetValue(new Circle(true, Color.Gray)));
            drawingMenu.AddItem(new MenuItem("rColor", "R Color").SetValue(new Circle(true, Color.Gray)));
            Menu.AddSubMenu(drawingMenu);

            // Misc
            var miscMenu = new Menu("[Chewy's Lux] - Misc", "cmLuxMisc");
            miscMenu.AddItem(new MenuItem("antiGapCloserQ", "Stun all gap closers").SetValue(true));
            miscMenu.AddItem(new MenuItem("packetCast", "Use packets for spells").SetValue(false));
            miscMenu.AddItem(
                new MenuItem("autoShield", "Auto-shield allies").SetValue(new KeyBind('c', KeyBindType.Toggle)));
            miscMenu.AddItem(new MenuItem("autoShieldPercent", "Auto Shield %").SetValue(new Slider(20)));
            miscMenu.AddItem(new MenuItem("debug", "Debug").SetValue(false));
            Menu.AddSubMenu(miscMenu);

            // Combo / Harass
            //Menu.AddItem(new MenuItem("combo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            //Menu.AddItem(new MenuItem("harass", "Harass!").SetValue(new KeyBind('v', KeyBindType.Press)));

            // Finalize
            Menu.AddToMainMenu();
        }
    }
}