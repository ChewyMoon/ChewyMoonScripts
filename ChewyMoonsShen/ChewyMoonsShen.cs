using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChewyMoon.Utility;
using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoonsShen
{
    class ChewyMoonsShen
    {
        public static Spell Q, W, E, R;
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;

        public static void OnGameLoad(EventArgs args)
        {
            // Setup skillshots
            Q = SpellHelper.CreateTargettedSpell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = SpellHelper.CreateSkillshotSpell(SpellSlot.E, SkillshotType.SkillshotLine, false);
            R = new Spell(SpellSlot.R);
                       
            SetupMenu();
        }

        private static void SetupMenu()
        {
            Menu = new Menu("[ChewyMoon - Shen]", "cmShen", true);

            // Target selector
            var tsMenu = new Menu("Target Selector", "cmShenTS");
            SimpleTs.AddToMenu(tsMenu);
            Menu.AddSubMenu(Menu);

            // Orbwalker
            var orbwalkMenu = new Menu("Orbwalker", "cmShenOrbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            // Combo
            var comboMenu = new Menu("Combo", "cmShenCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(false));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useWWhenHealth", "Use W if health below %").SetValue(true));
            comboMenu.AddItem(new MenuItem("useWHealth", "Health %").SetValue(new Slider(50)));
            comboMenu.AddItem(
                new MenuItem("useEFlash", "Use Flash E -> combo").SetValue(new KeyBind('t', KeyBindType.Press)));
            Menu.AddSubMenu(comboMenu);
            
            // Harass
            var harassMenu = new Menu("Harass", "cmShenHarass");
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "cmShenDrawing");
            drawMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(new Circle(true, Color.Crimson)));
            drawMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(new Circle(true, Color.Crimson)));
            drawMenu.AddItem(new MenuItem("drawBox", "Draw box").SetValue(true));

            // Ult stufffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
            var ultMenu = new Menu("Ult", "cmShenUlt");


            // misc
            var miscMenu = new Menu("Misc", "cmShen");
            miscMenu.AddItem(new MenuItem("wIncDamage", "W on incom. damage").SetValue(true));
            miscMenu.AddItem(new MenuItem("wIncDamagePercent", "Damage to shield").SetValue(new Slider(3)));
            Menu.AddSubMenu(miscMenu);

            Menu.AddToMainMenu();

        }
    }
}
