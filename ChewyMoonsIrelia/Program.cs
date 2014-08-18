using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoonsIrelia
{
    class Program
    {
        private const string ChampName = "Irelia";
        private const string Version = "0.1";

        private static Menu menu;
        private static Orbwalking.Orbwalker orbwalker;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampName)
                return;
            
            SetupMenu();

            //Done loading.
            Utilities.PrintChat("You've got the latest version(" + Version + ")");
        }

        private static void SetupMenu()
        {
            menu = new Menu("--[ChewyMoon's Irelia]--", "cmIrelia", true);

            // Target Selector
            var targetSelectorMenu = new Menu("[ChewyMoon's Irelia] - TS", "cmIreliaTS");
            SimpleTs.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);

            // Orbwalker
            var orbwalkerMenu = new Menu("[ChewyMoon's Irelia] - Orbwalking", "cmIreliaOW");
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);

            //Combo
            var comboMenu = new Menu("[ChewyMoon's Irelia] - Combo", "cmIreliaCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W in combo").SetValue(true));



            menu.AddToMainMenu();
        }
    }
}
