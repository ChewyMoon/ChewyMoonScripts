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

namespace AzirRange
{
    using System;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Called when the program starts.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        /// Called when the game loads.
        /// </summary>
        /// <param name="args">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        /// <summary>
        /// Called when the game is drawn.
        /// </summary>
        /// <param name="args">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private static void DrawingOnOnDraw(EventArgs args)
        {
            foreach (var soldier in
                ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsEnemy && x.CharData.BaseSkinName == "AzirSoldier"))
            {
                Render.Circle.DrawCircle(soldier.Position, 250, Color.LightBlue);
            }
        }
    }
}