// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ChewyMoon">
//   Copyright (C) 2015 ChewyMoon
//   
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   A champion that can ward jump.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Cya_Nerds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     A champion that can ward jump.
    /// </summary>
    internal struct WardJumpChampion
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WardJumpChampion" /> struct.
        /// </summary>
        /// <param name="champName">Name of the champ.</param>
        /// <param name="wardJumpSpell">The ward jump spell.</param>
        public WardJumpChampion(string champName, Spell wardJumpSpell)
        {
            this.ChampionName = champName;
            this.WardJumpSpell = wardJumpSpell;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the name of the champion.
        /// </summary>
        /// <value>
        ///     The name of the champion.
        /// </value>
        public string ChampionName { get; set; }

        /// <summary>
        ///     Gets or sets the ward jump spell.
        /// </summary>
        /// <value>
        ///     The ward jump spell.
        /// </value>
        public Spell WardJumpSpell { get; set; }

        #endregion
    }

    /// <summary>
    ///     The program.
    /// </summary>
    internal class Program
    {
        #region Static Fields

        /// <summary>
        ///     The last ward placed t
        /// </summary>
        private static int lastWardPlacedT;

        /// <summary>
        ///     The menu
        /// </summary>
        private static Menu menu;

        /// <summary>
        ///     The plugin
        /// </summary>
        private static WardJumpChampion plugin;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the ward jump champions.
        /// </summary>
        /// <value>
        ///     The ward jump champions.
        /// </value>
        public static List<WardJumpChampion> WardJumpChampions { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether we are ward jumping.
        /// </summary>
        /// <value>
        ///     <c>true</c> if we are ward jumping; otherwise, <c>false</c>.
        /// </value>
        private static bool WardJump
        {
            get
            {
                return menu.Item("wardJump").GetValue<KeyBind>().Active;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired when the game loads.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (WardJumpChampions.All(x => x.ChampionName != ObjectManager.Player.ChampionName))
            {
                return;
            }

            plugin = WardJumpChampions.First(x => x.ChampionName == ObjectManager.Player.ChampionName);

            menu = new Menu("Cya Nerds", "cmCyaNerds", true);
            menu.AddItem(new MenuItem("maxWardJump", "Jump to max range").SetValue(true));
            menu.AddItem(new MenuItem("jumpRange", "Existing Obj Range").SetValue(new Slider(250, 0, 700)));
            menu.AddItem(new MenuItem("wardDelay", "Ward Delay(MS)").SetValue(new Slider(3000, 0, 10 * 1000)));
            menu.AddItem(
                new MenuItem("wardJump", "Ward Jump").SetValue(new KeyBind("t".ToCharArray()[0], KeyBindType.Press)));
            menu.AddToMainMenu();

            GameObject.OnCreate += GameObjectOnOnCreate;
            Game.OnUpdate += GameOnOnGameUpdate;
            Game.PrintChat("<font color=\"#7CFC00\"><b>Cya Nerds:</b></font> by ChewyMoon loaded.");
        }

        /// <summary>
        ///     Fired when a <see cref="GameObject" /> is created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (!WardJump || !(sender is Obj_AI_Minion) || !sender.Name.ToUpper().Contains("WARD"))
            {
                return;
            }

            var ward = (Obj_AI_Minion)sender;
            if (sender.Position.Distance(ObjectManager.Player.ServerPosition) <= plugin.WardJumpSpell.Range)
            {
                plugin.WardJumpSpell.CastOnUnit(ward);
            }
        }

        /// <summary>
        ///     Fired when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnGameUpdate(EventArgs args)
        {
            if (!plugin.WardJumpSpell.IsReady() || !WardJump)
            {
                return;
            }

            var jumpRange = menu.Item("jumpRange").GetValue<Slider>().Value;

            // Jump to a ward
            var ward =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(x => x.Name.ToUpper().Contains("WARD") && x.Distance(Game.CursorPos) < jumpRange)
                    .OrderByDescending(x => x.Distance(Player))
                    .FirstOrDefault();

            if (ward != null)
            {
                plugin.WardJumpSpell.CastOnUnit(ward);
                return;
            }

            // Jump to ally
            var ally =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsAlly && !x.IsMe && x.Distance(Game.CursorPos) < jumpRange)
                    .OrderByDescending(x => x.Distance(Player))
                    .FirstOrDefault();

            if (ally != null)
            {
                plugin.WardJumpSpell.CastOnUnit(ally);
                return;
            }

            // Jump to minion
            var minion =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(x => x.IsAlly && x.Distance(Game.CursorPos) < jumpRange)
                    .OrderByDescending(x => x.Distance(Player))
                    .FirstOrDefault();

            if (minion != null)
            {
                plugin.WardJumpSpell.CastOnUnit(minion);
                return;
            }

            // Place ward to jump to
            var wardSlot = Items.GetWardSlot();
            if (Environment.TickCount < lastWardPlacedT + menu.Item("wardDelay").GetValue<Slider>().Value
                || !plugin.WardJumpSpell.IsReady() || plugin.WardJumpSpell.Instance.SData.Name == "blindmonkwtwo"
                || !Items.CanUseItem((int)wardSlot.Id) || wardSlot.Stacks == 0)
            {
                return;
            }

            var placeAtMaxRange = menu.Item("maxWardJump").GetValue<bool>();
            var pos = Game.CursorPos;
            const int Range = 600;

            if (!placeAtMaxRange)
            {
                Items.UseItem((int)wardSlot.Id, Game.CursorPos);
                lastWardPlacedT = Environment.TickCount;
                return;
            }

            // extend the mouse pos
            var placePos = ObjectManager.Player.ServerPosition.To2D().Extend(pos.To2D(), Range);
            Items.UseItem((int)wardSlot.Id, placePos);
            lastWardPlacedT = Environment.TickCount;
        }

        /// <summary>
        ///     The entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            WardJumpChampions = new List<WardJumpChampion>
                                    {
                                        new WardJumpChampion("LeeSin", new Spell(SpellSlot.W, 700)), 
                                        new WardJumpChampion("Katarina", new Spell(SpellSlot.E, 700)), 
                                        new WardJumpChampion("Jax", new Spell(SpellSlot.Q, 700))
                                    };

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion
    }
}