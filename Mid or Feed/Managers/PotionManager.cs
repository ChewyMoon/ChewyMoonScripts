#region



#endregion

namespace Mid_or_Feed.Managers
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class PotionManager : Manager
    {
        #region Constants

        /// <summary>
        ///     The health potion id.
        /// </summary>
        private const int Hpid = 2003;

        /// <summary>
        ///     The hp pot name
        /// </summary>
        private const string HpPotName = "RegenerationPotion";

        /// <summary>
        ///     The the mana potion id.
        /// </summary>
        private const int Mpid = 2004;

        /// <summary>
        ///     The mp pot name
        /// </summary>
        private const string MpPotName = "FlaskOfCrystalWater";

        #endregion

        #region Static Fields

        /// <summary>
        ///     The menu
        /// </summary>
        private static Menu menu;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Loads the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Load(Menu config)
        {
            config.AddItem(new MenuItem("useHP", "Use Health Pot").SetValue(true));
            config.AddItem(new MenuItem("useHPPercent", "Health %").SetValue(new Slider(35, 1)));
            config.AddItem(new MenuItem("sseperator", "       "));
            config.AddItem(new MenuItem("useMP", "Use Mana Pot").SetValue(true));
            config.AddItem(new MenuItem("useMPPercent", "Mana %").SetValue(new Slider(35, 1)));

            menu = config;

            Game.OnUpdate += GameOnOnGameUpdate;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnGameUpdate(EventArgs args)
        {
            var useHp = menu.Item("useHP").GetValue<bool>();
            var useMp = menu.Item("useMP").GetValue<bool>();

            if (ObjectManager.Player.InFountain())
            {
                return;
            }

            if (useHp && ObjectManager.Player.HealthPercent <= menu.Item("useHPPercent").GetValue<Slider>().Value
                && !HasHealthPotBuff())
            {
                // Cast health pot
                if (Items.CanUseItem(Hpid) && Items.HasItem(Hpid))
                {
                    Items.UseItem(Hpid);
                }
            }

            if (!useMp || !(ObjectManager.Player.ManaPercent <= menu.Item("useMPPercent").GetValue<Slider>().Value)
                || HasMannaPutBuff())
            {
                return;
            }

            if (Items.CanUseItem(Mpid) && Items.HasItem(Mpid))
            {
                Items.UseItem(Mpid);
            }
        }

        /// <summary>
        ///     Determines whether the player has the health pot buff.
        /// </summary>
        /// <returns></returns>
        private static bool HasHealthPotBuff()
        {
            return ObjectManager.Player.HasBuff(HpPotName);
        }

        /// <summary>
        ///     Determines whether the player has the manna put buff.
        /// </summary>
        /// <returns></returns>
        private static bool HasMannaPutBuff()
        {
            return ObjectManager.Player.HasBuff(MpPotName);
        }

        #endregion
    }
}