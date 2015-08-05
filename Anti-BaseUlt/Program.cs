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
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Anti_BaseUlt
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    /// <summary>
    ///     The program.
    /// </summary>
    internal class Program
    {
        #region Static Fields

        /// <summary>
        ///     The base ult names.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static readonly string[] BaseUltNames =
            {
                "EzrealTrueshotBarrage", "EnchantedCrystalArrow", 
                "DravenDoubleShotMissile", "JinxR"
            };

        /// <summary>
        ///     The last time the user was notified.
        /// </summary>
        private static int lastNotification;

        /// <summary>
        ///     The time your recall finishes
        /// </summary>
        private static int recallFinishTime;

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

        #endregion

        #region Methods

        /// <summary>
        ///     Called when the game loads.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnGameLoad(EventArgs args)
        {
            Game.PrintChat("<font color=\"#7CFC00\"><b>Anti-BaseUlt:</b></font> by ChewyMoon loaded");

            Game.OnUpdate += GameOnOnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        /// <summary>
        ///     Called when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnUpdate(EventArgs args)
        {
            if (!Player.IsRecalling())
            {
                return;
            }

            foreach (var missile in
                ObjectManager.Get<MissileClient>()
                    .Where(x => BaseUltNames.Any(y => y.Equals(x.SData.Name)) && x.SpellCaster.IsEnemy))
            {
                Console.WriteLine("DETECTED");
                var spellCaster = missile.SpellCaster as Obj_AI_Hero;

                // Wont kill us. Assume for jinx that the ult will do maximum damage.
                if (spellCaster.GetSpellDamage(Player, SpellSlot.R, missile.SData.Name.Equals("JinxR") ? 1 : 0)
                    < Player.Health)
                {
                    Console.WriteLine("Wont kill us!");
                    continue;
                }

                // Not a baseult
                if (!PositionIsInFountain(missile.EndPosition))
                {
                    Console.WriteLine("Position not in fountain!");
                    continue;
                }

                var objSpawnPoint = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsAlly);
                if (objSpawnPoint == null)
                {
                    continue;
                }

                var timeToFountain = missile.Position.Distance(objSpawnPoint.Position) / missile.SData.MissileSpeed
                                     * 1000;

                // Not timed correctly.
                if (recallFinishTime < timeToFountain)
                {
                    Console.WriteLine("Not correctly timed!");
                    continue;
                }

                Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position);

                if (Environment.TickCount - lastNotification > 1000)
                {
                    Game.PrintChat("<font color=\"#7CFC00\"><b>Anti-BaseUlt:</b></font> prevented a baseult!");
                    lastNotification = Environment.TickCount;

                    break;
                }
            }
        }

        /// <summary>
        ///     The main.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        /// <summary>
        ///     Called when the game processes spell casts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValid<Obj_AI_Hero>())
            {
                return;
            }

            var hero = (Obj_AI_Hero)sender;

            if (!hero.IsMe || args.SData.Name != "recall")
            {
                return;
            }

            recallFinishTime = Environment.TickCount + Utility.GetRecallTime(hero);
            Console.WriteLine("Detected Recall");
        }

        /// <summary>
        ///     Checks if the position is in the fountain.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>If the position is in the fountain.</returns>
        private static bool PositionIsInFountain(Vector3 position)
        {
            float fountainRange = 562500;
            var map = Utility.Map.GetMap();

            if (map != null && map.Type == Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1102500;
            }

            return
                ObjectManager.Get<Obj_SpawnPoint>()
                    .Any(x => x.Team == Player.Team && position.Distance(x.Position, true) < fountainRange);
        }

        #endregion
    }
}