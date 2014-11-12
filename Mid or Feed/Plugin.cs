using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Mid_or_Feed
{
    public abstract class Plugin
    {
        public Menu Menu { get; internal set; }

        public Orbwalking.Orbwalker Orbwalker { get; internal set; }

        public Orbwalking.OrbwalkingMode OrbwalkerMode { get { return Orbwalker.ActiveMode; } }

        public bool Packets { get { return GetValue<bool>("packets"); } }

        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        protected Plugin()
        {
            CreateMenu();
            Utility.HpBarDamageIndicator.DamageToUnit = DamageToUnit;
            Utility.HpBarDamageIndicator.Enabled = true;
            PrintChat("loading. Created by ChewyMoon :3");
        }

        private float DamageToUnit(Obj_AI_Hero hero)
        {
            return GetComboDamage(hero);
        }

        private void CreateMenu()
        {
            Menu = new Menu("Mid or Feed", "mof", true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "mofTS");
            SimpleTs.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkMenu = new Menu("Orbwalker", "mofOrbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            // Combo
            var comboMenu = new Menu("Combo", "mofCombo");
            Combo(comboMenu);
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "mofHarass");
            Harass(harassMenu);
            Menu.AddSubMenu(harassMenu);

            // Items
            var itemsMenu = new Menu("Items", "mofItems");
            Items(itemsMenu);
            Menu.AddSubMenu(itemsMenu);

            // Misc
            var miscMenu = new Menu("Misc", "mofMisc");
            miscMenu.AddItem(new MenuItem("packets", "Use packets").SetValue(true));
            Misc(miscMenu);
            Menu.AddSubMenu(miscMenu);

            // Drawing
            var drawingMenu = new Menu("Drawings", "mofDrawing");
            Drawings(drawingMenu);
            Menu.AddSubMenu(drawingMenu);

            Menu.AddToMainMenu();
        }

        public static void PrintChat(string msg)
        {
            Game.PrintChat("<font color='#3492EB'>Mid or Feed:</font> <font color='#FFFFFF'>" + msg + "</font>");
        }

        public T GetValue<T>(string name)
        {
            return Menu.Item(name).GetValue<T>();
        }

        public virtual float GetComboDamage(Obj_AI_Hero target)
        {
            return 0;
        }

        public Spell GetSpell(List<Spell> spellList, SpellSlot slot)
        {
            return spellList.First(x => x.Slot == slot);
        }

        #region Virtuals

        public virtual void Combo(Menu comboMenu)
        {
        }

        public virtual void Harass(Menu harassMenu)
        {
        }

        public virtual void Items(Menu itemsMenu)
        {
        }

        public virtual void Misc(Menu miscMenu)
        {
        }

        public virtual void Drawings(Menu drawingMenu)
        {
        }

        #endregion Virtuals
    }
}