// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BallManager.cs" company="ChewyMoon">
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
//   Handles Orianna's ball. Credits to Kortatu / Esk0r.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Orianna
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    /// <summary>
    ///     Handles Orianna's ball. Credits to Kortatu / Esk0r.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
        Justification = "Reviewed. Suppression is OK here.")]
    internal class BallManager
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="BallManager" /> class.
        /// </summary>
        static BallManager()
        {
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;

            BallPosition =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(x => x.IsAlly && x.CharData.BaseSkinName.Equals("oriannaball"))
                    .Select(x => x.Position)
                    .FirstOrDefault();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the ball position.
        /// </summary>
        /// <value>
        ///     The ball position.
        /// </value>
        public static Vector3 BallPosition { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Called when the game updates
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.HasBuff("OrianaGhostSelf"))
            {
                BallPosition = ObjectManager.Player.Position;
            }

            foreach (var ally in HeroManager.Allies.Where(ally => ally.HasBuff("OrianaGhost")))
            {
                BallPosition = ally.Position;
                break;
            }
        }

        /// <summary>
        ///     Called when the game processes spell casts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            switch (args.SData.Name)
            {
                case "OrianaIzunaCommand":
                    Utility.DelayAction.Add(
                        (int)((BallPosition.Distance(args.End) / 1.2) - 70 - Game.Ping), 
                        () => BallPosition = args.End);
                    BallPosition = Vector3.Zero;
                    break;

                case "OrianaRedactCommand":
                    BallPosition = Vector3.Zero;
                    break;
            }
        }

        #endregion
    }
}