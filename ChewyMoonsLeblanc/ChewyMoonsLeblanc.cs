using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoonsLeblanc
{
    class ChewyMoonsLeblanc
    {
        private static Menu menu;
        private static Spell Q, W, E, R;
        private static Spell lastSpell = null;

        internal static void OnGameLoad(EventArgs args)
        {
            // currentHealth / maxHealth * 100 = percent MATHG33k

            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 950);

            Q.SetTargetted(0.5f, 2000f);
            W.SetSkillshot();

            SetupMenu();
        }

        private static void SetupMenu()
        {
            // Setup main menu
            menu = new Menu("[ChewyMoon - Leblanc]", "cmLeblanc", true);

            // Combo settings
            var comboMenu = new Menu("[ChewyMoon's Leblanc] - Combo", "cmLeblancCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useQR", "Use R (Q)").SetValue(true));
            menu.AddSubMenu(comboMenu);

            // Item settings
            var itemMenu = new Menu("[ChewyMoon's Leblanc] - Items", "cmLeblancItems");
            itemMenu.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
            itemMenu.AddItem(new MenuItem("useHourglass", "Use Zhonya's Hourglass").SetValue(true));
            itemMenu.AddItem(new MenuItem("lifePercent", "Min health %").SetValue(new Slider(10)));
            menu.AddSubMenu(itemMenu);

            // Farming Settings
            var farmingMenu = new Menu("[ChewyMoon's Leblanc] - Farming", "cmLeblancFarming");
            farmingMenu.AddItem(new MenuItem("qFarm", "Farm with Q").SetValue(false));
            farmingMenu.AddItem(new MenuItem("qFarmPercent", "Min mana %").SetValue(new Slider(15)));
            menu.AddSubMenu(farmingMenu);

            // Combo
            menu.AddItem(new MenuItem("combo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Finalize
            menu.AddToMainMenu();
        }
    }
}
