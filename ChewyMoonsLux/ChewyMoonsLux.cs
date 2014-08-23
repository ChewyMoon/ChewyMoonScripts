using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ChewyMoonsLux
{
    class ChewyMoonsLux
    {
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static bool PacketCast = false;
        public static void OnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            // Refine skillshots
            Q.SetSkillshot(0.25f, 80f, 1200f, true, Prediction.SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 150f, 1200f, false, Prediction.SkillshotType.SkillshotLine);
            E.SetSkillshot(0.15f, 275f, 1300f, false, Prediction.SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.7f, 190f, float.MaxValue, false, Prediction.SkillshotType.SkillshotLine);

            // Setup Main Menu
            SetupMenu();

            // Checkerino for updates
            LuxUpdater.CheckForUpdates();

            // Updaterino
            Game.OnGameUpdate += LuxCombo.OnGameUpdate;

            // Draw
            Drawing.OnDraw += OnDraw;

            //AntiGapCloser
            AntiGapcloser.OnEnemyGapcloser += QGapCloser.OnEnemyGapCloser;

            // Orbwalker
            Orbwalking.AfterAttack += LuxCombo.AfterAttack;
        }

        private static void OnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("drawQ").GetValue<bool>();
            var drawW = Menu.Item("drawW").GetValue<bool>();
            var drawE = Menu.Item("drawE").GetValue<bool>();
            var drawR = Menu.Item("drawR").GetValue<bool>();

            var qColor = Menu.Item("qColor").GetValue<Color>();
            var wColor = Menu.Item("wColor").GetValue<Color>();
            var eColor = Menu.Item("eColor").GetValue<Color>();
            var rColor = Menu.Item("rColor").GetValue<Color>();

            var position = ObjectManager.Player.Position;

            if(drawQ)
                Utility.DrawCircle(position, Q.Range, qColor);

            if (drawW)
                Utility.DrawCircle(position, W.Range, wColor);                

            if (drawE)
                Utility.DrawCircle(position, E.Range, eColor);

            if (drawR)
                Utility.DrawCircle(position, R.Range, rColor);

        }

        private static void SetupMenu()
        {
            Menu = new Menu("--[ChewyMoon's Lux]--", "cmLux", true);

            // Target Selector
            var tsMenu = new Menu("[ChewyMoon's Lux] - TS", "cmLuxTs");
            SimpleTs.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkerMenu = new Menu("[ChewyMoon's Lux] - Orbwalker", "cmLuxOrbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            // Combo settings
            var comboMenu = new Menu("[ChewyMoon's Lux] - Combo", "cmLuxCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(false));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("aaAfterSpell", "AA after spell").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass Settings
            var harassMenu = new Menu("[ChewyMoon's Lux] - Harass", "cmLuxHarass");
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("aaHarass", "Auto attack after harass").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // KS / Finisher Settings
            var ksMenu = new Menu("[ChewyMoon's Lux] - KS", "cmLuxKS");
            ksMenu.AddItem(new MenuItem("ultKS", "KS with R").SetValue(true));
            ksMenu.AddItem(new MenuItem("recallExploitKS", "KS enemies recalling").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            // Items
            var itemsMenu = new Menu("[ChewyMoon's lux] - Items", "cmLuxItems");
            itemsMenu.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
            Menu.AddSubMenu(itemsMenu);

            //Drawing
            var drawingMenu = new Menu("[ChewyMoon's Lux] - Drawing", "cmLuxDrawing");
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
            var miscMenu = new Menu("[ChewyMoon's Lux] - Misc", "cmLuxMisc");
            miscMenu.AddItem(new MenuItem("antiGapCloserQ", "Stun all gap closers").SetValue(true));
            miscMenu.AddItem(new MenuItem("packetCast", "Use packets for spells").SetValue(false));
            miscMenu.AddItem(new MenuItem("autoShield", "Auto-shield allies").SetValue(new KeyBind('c', KeyBindType.Toggle)));
            miscMenu.AddItem(new MenuItem("autoShieldPercent", "Auto Shield %").SetValue(new Slider(20)));
            Menu.AddSubMenu(miscMenu);

            // Combo
            Menu.AddItem(new MenuItem("combo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Finalize
            Menu.AddToMainMenu();
        }
    }
}
