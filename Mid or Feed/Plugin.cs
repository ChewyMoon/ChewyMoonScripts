#region



#endregion

namespace Mid_or_Feed
{
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using Mid_or_Feed.Managers;

    /// <summary>
    ///     Represents a Plugin.
    /// </summary>
    public abstract class Plugin
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Plugin" /> class.
        /// </summary>
        protected Plugin()
        {
            this.CreateMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = this.DamageToUnit;
            Utility.HpBarDamageIndicator.Enabled = true;

            PrintChat("loading. Created by ChewyMoon :3");
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public Menu Menu { get; internal set; }

        /// <summary>
        ///     Gets the orbwalker.
        /// </summary>
        /// <value>
        ///     The orbwalker.
        /// </value>
        public Orbwalking.Orbwalker Orbwalker { get; internal set; }

        /// <summary>
        ///     Gets the orbwalker mode.
        /// </summary>
        /// <value>
        ///     The orbwalker mode.
        /// </value>
        public Orbwalking.OrbwalkingMode OrbwalkerMode
        {
            get
            {
                return this.Orbwalker.ActiveMode;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Plugin" /> is using packets.
        /// </summary>
        /// <value>
        ///     <c>true</c> if using packets; otherwise, <c>false</c>.
        /// </value>
        public bool Packets
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        public Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Prints to the chat.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void PrintChat(string msg)
        {
            Game.PrintChat("<font color='#3492EB'>Mid or Feed:</font> <font color='#FFFFFF'>" + msg + "</font>");
        }

        /// <summary>
        ///     Creates the combo menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public virtual void Combo(Menu config)
        {
        }

        /// <summary>
        ///     Creates the drawing menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public virtual void Drawings(Menu config)
        {
        }

        /// <summary>
        ///     Gets the bool.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool GetBool(string name)
        {
            return this.GetValue<bool>(name);
        }

        /// <summary>
        ///     Gets the combo damage.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public virtual float GetComboDamage(Obj_AI_Hero target)
        {
            return 0;
        }

        /// <summary>
        ///     Gets the spell.
        /// </summary>
        /// <param name="spellList">The spell list.</param>
        /// <param name="slot">The slot.</param>
        /// <returns></returns>
        public Spell GetSpell(List<Spell> spellList, SpellSlot slot)
        {
            return spellList.First(x => x.Slot == slot);
        }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public T GetValue<T>(string name)
        {
            return this.Menu.Item(name).GetValue<T>();
        }

        /// <summary>
        ///     Creates the harass menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public virtual void Harass(Menu config)
        {
        }

        /// <summary>
        ///     Creates the item menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public virtual void ItemMenu(Menu config)
        {
        }

        /// <summary>
        ///     Creates the misc menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public virtual void Misc(Menu config)
        {
        }

        /// <summary>
        ///     Creates the wave clear menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public virtual void WaveClear(Menu config)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            this.Menu = new Menu("Mid or Feed", "mof" + this.Player.ChampionName, true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "mofTS");
            TargetSelector.AddToMenu(tsMenu);
            this.Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkMenu = new Menu("Orbwalker", "mofOrbwalker");
            this.Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            this.Menu.AddSubMenu(orbwalkMenu);

            // Combo
            var comboMenu = new Menu("Combo", "mofCombo");
            this.Combo(comboMenu);
            this.Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "mofHarass");
            this.Harass(harassMenu);
            this.Menu.AddSubMenu(harassMenu);

            // Wave Clear
            var waveClearMenu = new Menu("Wave Clear", "mofWaveClear");
            this.WaveClear(waveClearMenu);
            this.Menu.AddSubMenu(waveClearMenu);

            // Items
            var itemsMenu = new Menu("Items", "mofItems");
            this.ItemMenu(itemsMenu);
            this.Menu.AddSubMenu(itemsMenu);

            // Misc
            var miscMenu = new Menu("Misc", "mofMisc");
            miscMenu.AddItem(new MenuItem("packets", "Use packets").SetValue(true));
            this.Misc(miscMenu);
            this.Menu.AddSubMenu(miscMenu);

            // Managers

            // Auto Ignite
            if (this.Player.GetSpellSlot("SummonerDot") != SpellSlot.Unknown)
            {
                var igniteMenu = new Menu("Ignite", "mofIgnite");
                new AutoIgnite().Load(igniteMenu);
                this.Menu.AddSubMenu(igniteMenu);
            }

            // Potion Manager
            var pmManager = new Menu("Potion Manager", "mofPM");
            new PotionManager().Load(pmManager);
            this.Menu.AddSubMenu(pmManager);

            // Drawing
            var drawingMenu = new Menu("Drawings", "mofDrawing");
            this.Drawings(drawingMenu);
            this.Menu.AddSubMenu(drawingMenu);

            this.Menu.AddToMainMenu();
        }

        /// <summary>
        ///     Gets the damage to a unit.
        /// </summary>
        /// <param name="hero">The hero.</param>
        /// <returns></returns>
        private float DamageToUnit(Obj_AI_Hero hero)
        {
            return this.GetComboDamage(hero);
        }

        #endregion
    }
}