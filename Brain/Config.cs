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

        public static bool CalcItems
        {
            get { return Item("cmBrainMItems"); }
        }

        public static bool CalcEnemyItems
        {
            get { return Item("cmBrainEItems"); }
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

            // Seperator
            _menu.AddItem(new MenuItem("", "cmBrainSeperator"));

            // Options
            _menu.AddItem(new MenuItem("cmBrainMDMG", "Calc. Minion Damage").SetValue(true));
            _menu.AddItem(new MenuItem("cmBrainMPercent", "Calc. Percents").SetValue(true));
            _menu.AddItem(new MenuItem("cmBrainPSpells", "Calc. My Spells").SetValue(true));
            _menu.AddItem(new MenuItem("cmBrainESpells", "Calc. Enemy Spells").SetValue(true));
            _menu.AddItem(new MenuItem("cmBrainMItems", "Calc. My Items").SetValue(true));
            _menu.AddItem(new MenuItem("cmBrainEItems", "Calc. Enemy Items").SetValue(true));
            _menu.AddItem(new MenuItem("cmBrainMSumSpells", "Calc. My Summoner Spells").SetValue(true));
            _menu.AddItem(new MenuItem("cmBrainESumSpells", "Calc. Enemy Summoner Spells").SetValue(true));

            _menu.AddToMainMenu();
        }

        private static bool Item(string name)
        {
            return _menu.Item(name).GetValue<bool>();
        }
    }
}