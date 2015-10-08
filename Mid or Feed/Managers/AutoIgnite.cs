#region



#endregion

namespace Mid_or_Feed.Managers
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     Automaticly ignites targets.
    /// </summary>
    internal class AutoIgnite : Manager
    {
        #region Constants

        /// <summary>
        ///     The ignite range
        /// </summary>
        private const int IgniteRange = 600;

        #endregion

        #region Fields

        /// <summary>
        ///     The ignite slot
        /// </summary>
        private SpellSlot igniteSlot;

        /// <summary>
        ///     The_menu
        /// </summary>
        private Menu menu;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Loads the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Load(Menu config)
        {
            config.AddItem(new MenuItem("igniteKill", "Use Ignite if Killable").SetValue(true));
            config.AddItem(new MenuItem("igniteKS", "Use Ignite KS").SetValue(true));

            this.menu = config;
            this.igniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            Game.OnUpdate += this.GameOnOnGameUpdate;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired when teh game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameOnOnGameUpdate(EventArgs args)
        {
            var igniteKill = this.menu.Item("igniteKill").GetValue<bool>();
            var igniteKs = this.menu.Item("igniteKS").GetValue<bool>();

            if (!this.igniteSlot.IsReady())
            {
                return;
            }

            if (igniteKill)
            {
                var igniteKillableEnemy =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(IgniteRange))
                        .FirstOrDefault(
                            x => ObjectManager.Player.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite) > x.Health);

                if (igniteKillableEnemy.IsValidTarget())
                {
                    ObjectManager.Player.Spellbook.CastSpell(this.igniteSlot, igniteKillableEnemy);
                }
            }

            if (!igniteKs)
            {
                return;
            }

            var enemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget(IgniteRange))
                    .FirstOrDefault(
                        x => x.Health <= ObjectManager.Player.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite) / 5);

            if (enemy.IsValidTarget())
            {
                ObjectManager.Player.Spellbook.CastSpell(this.igniteSlot, enemy);
            }
        }

        #endregion
    }
}