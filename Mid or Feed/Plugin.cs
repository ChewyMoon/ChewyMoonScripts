#region

using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Mid_or_Feed.Managers;

#endregion

namespace Mid_or_Feed
{
    public abstract class Plugin
    {
        protected Plugin()
        {
            CreateMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = DamageToUnit;
            Utility.HpBarDamageIndicator.Enabled = true;

            PrintChat("loading. Created by ChewyMoon :3");
        }

        public Menu Menu { get; internal set; }
        public Orbwalking.Orbwalker Orbwalker { get; internal set; }

        public Orbwalking.OrbwalkingMode OrbwalkerMode
        {
            get { return Orbwalker.ActiveMode; }
        }

        public bool Packets
        {
            get { return false; }
        }

        public Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private float DamageToUnit(Obj_AI_Hero hero)
        {
            return GetComboDamage(hero);
        }

        private void CreateMenu()
        {
            Menu = new Menu("Mid or Feed", "mof" + Player.ChampionName, true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "mofTS");
            TargetSelector.AddToMenu(tsMenu);
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
            ItemMenu(itemsMenu);
            Menu.AddSubMenu(itemsMenu);

            // Misc
            var miscMenu = new Menu("Misc", "mofMisc");
            miscMenu.AddItem(new MenuItem("packets", "Use packets").SetValue(true));
            Misc(miscMenu);
            Menu.AddSubMenu(miscMenu);

            // Managers

            // Auto Ignite
            if (Player.GetSpellSlot("SummonerDot") != SpellSlot.Unknown)
            {
                var igniteMenu = new Menu("Ignite", "mofIgnite");
                new AutoIgnite().Load(igniteMenu);
                Menu.AddSubMenu(igniteMenu);
            }

            // Potion Manager
            var pmManager = new Menu("Potion Manager", "mofPM");
            new PotionManager().Load(pmManager);
            Menu.AddSubMenu(pmManager);

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

        public bool GetBool(string name)
        {
            return GetValue<bool>(name);
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

        public virtual void Combo(Menu config) {}

        public virtual void Harass(Menu config) {}

        public virtual void ItemMenu(Menu config) {}

        public virtual void Misc(Menu config) {}

        public virtual void Drawings(Menu config) {}

        #endregion Virtuals
    }
}