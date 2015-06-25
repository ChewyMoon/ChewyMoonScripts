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
// --------------------------------------------------------------------------------------------------------------------

namespace InsaneEzreal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The main program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Gets or sets the q.
        /// </summary>
        /// <value>
        /// The q.
        /// </value>
        private static Spell Q { get; set; }

        /// <summary>
        /// Gets or sets the w.
        /// </summary>
        /// <value>
        /// The w.
        /// </value>
        private static Spell W { get; set; }

        /// <summary>
        /// Gets or sets the e.
        /// </summary>
        /// <value>
        /// The e.
        /// </value>
        private static Spell E { get; set; }

        /// <summary>
        /// Gets or sets the r.
        /// </summary>
        /// <value>
        /// The r.
        /// </value>
        private static Spell R { get; set; }

        /// <summary>
        /// Gets or sets the menu.
        /// </summary>
        /// <value>
        /// The menu.
        /// </value>
        private static Menu Menu { get; set; }

        /// <summary>
        /// Gets or sets the orbwalker.
        /// </summary>
        /// <value>
        /// The orbwalker.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private static Orbwalking.Orbwalker Orbwalker { get; set; }

        /// <summary>
        /// Gets the player.
        /// </summary>
        /// <value>
        /// The player.
        /// </value>
        public static Obj_AI_Hero Player { get
        {
            return ObjectManager.Player;
        } }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        /// The on game load.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W, 1050);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.251f, 60, 2000, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.539f, 80, 1600, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.75f, 160, 2000, false, SkillshotType.SkillshotLine);

            CreateMenu();

            Game.OnUpdate += Game_OnUpdate;
        }

        /// <summary>
        /// Creates the menu.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private static void CreateMenu()
        {
            Menu = new Menu("InsaneEzreal", "chewyE");

            // Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(Menu);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Combo
            var comboMenu = new Menu("Combo", "combo shit");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("MaxR", "Maximum R Range").SetValue(new Slider(1500, 0, 5000)));
            comboMenu.AddItem(new MenuItem("MinR", "Minimum R Range").SetValue(new Slider(600, 0, 1000)));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "MenuMakingIsSOOOOOOOBOring");
            harassMenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("HarassMana", "Mana Percentage").SetValue(new Slider()));
            harassMenu.AddItem(new MenuItem("HarassToggle", "Harass! (Toggle)").SetValue(new KeyBind(84, KeyBindType.Press)));
            Menu.AddSubMenu(harassMenu);

            Menu.AddToMainMenu();
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static T GetValue<T>(string name)
        {
            return Menu.Item(name).GetValue<T>();
        }

        /// <summary>
        /// The game on update.
        /// </summary>
        /// <param name="args">
        ///  The args.
        /// </param>
        private static void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LastHit:
                    DoLastHit();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    DoWaveClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }
        }

        /// <summary>
        /// Does the combo.
        /// </summary>
        private static void DoCombo()
        {
            var useQ = GetValue<bool>("UseQCombo");
            var useW = GetValue<bool>("UseWCombo");
            var useE = GetValue<bool>("UseECombo");
            var useR = GetValue<bool>("UseRCombo");

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target);
            }

            if (useW && W.IsReady())
            {
                W.Cast(target);
            }

            var circleIntersection = Geometry.CircleCircleIntersection(
                Player.ServerPosition.To2D(),
                Prediction.GetPrediction(target, 0.25f).UnitPosition.To2D(),
                E.Range,
                Orbwalking.GetRealAutoAttackRange(target));

            if (useE && E.IsReady() && circleIntersection.Count() > 0)
            {
                E.Cast(circleIntersection.FirstOrDefault());
            }

            var targDistance = Player.Distance(target);
            if (useR && R.IsReady() && targDistance < GetValue<Slider>("MaxR").Value
                && targDistance > GetValue<Slider>("MinR").Value)
            {
                R.Cast(target);
            }
        }

        /// <summary>
        /// Does the wave clear.
        /// </summary>
        private static void DoWaveClear()
        {
            
        }

        /// <summary>
        /// Does the harass.
        /// </summary>
        private static void DoHarass()
        {
            
        }

        /// <summary>
        /// Does the last hit.
        /// </summary>
        private static void DoLastHit()
        {
            
        }
    }
}
