using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain
{
    internal class Config
    {
        private static Menu _menu;

        #region Config Values

        public static bool CalcMinionDamage
        {
            get { return Item("cmBrainMDMG"); }
        }

        public static bool CalcPercent
        {
            get { return Item("cmBrainPercent"); }
        }

        public static bool CalcSpells
        {
            get { return Item("cmBrainPSpells"); }
        }

        public static bool CalcEnemySpells
        {
            get { return Item("cmBrainESpells"); }
        }

        public static bool CalcSummonerSpells
        {
            get { return Item("cmBrainMSumSpells"); }
        }

        public static bool CalcEnemySummonerSpells
        {
            get { return Item("cmBrainESumSpells"); }
        }

        #endregion Config Values

        public static void CreateMenu()
        {
            _menu = new Menu("Brain", "cmBrain", true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "cmBrainTS");
            SimpleTs.AddToMenu(tsMenu);
            _menu.AddSubMenu(tsMenu);

            // SubMenu
            _menu.AddSubMenu(new Menu("Calculate", "cmCalculate"));

            // Options
            _menu.SubMenu("cmCalculate").AddItem(new MenuItem("cmBrainMDMG", "Minion Damage").SetValue(true));
            //_menu.SubMenu("cmCalculate").AddItem(new MenuItem("cmBrainMPercent", "Percents").SetValue(true));
            _menu.SubMenu("cmCalculate").AddItem(new MenuItem("cmBrainPSpells", "My Spells").SetValue(true));
            _menu.SubMenu("cmCalculate").AddItem(new MenuItem("cmBrainESpells", "Enemy Spells").SetValue(true));
            _menu.SubMenu("cmCalculate").AddItem(new MenuItem("cmBrainMSumSpells", "My Summoner Spells").SetValue(true));
            _menu.SubMenu("cmCalculate").AddItem(new MenuItem("cmBrainESumSpells", "Enemy Summoner Spells").SetValue(true));

            _menu.AddToMainMenu();
        }

        private static bool Item(string name)
        {
            return _menu.SubMenu("cmCalculate").Item(name).GetValue<bool>();
        }
    }
}