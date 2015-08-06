// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SophiesSoraka.cs" company="ChewyMoon">
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
//   The sophies soraka.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Sophies_Soraka
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The sophies soraka.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    internal class SophiesSoraka
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the e.
        /// </summary>
        /// <value>
        /// The e.
        /// </value>
        public static Spell E { get; set; }

        /// <summary>
        /// Gets or sets the menu.
        /// </summary>
        /// <value>
        /// The menu.
        /// </value>
        public static Menu Menu { get; set; }

        /// <summary>
        /// Gets or sets the orbwalker.
        /// </summary>
        /// <value>
        /// The orbwalker.
        /// </value>
        public static Orbwalking.Orbwalker Orbwalker { get; set; }

        /// <summary>
        ///     Gets a value indicating whether to use packets.
        /// </summary>
        public static bool Packets
        {
            get
            {
                return Menu.Item("packets").GetValue<bool>();
            }
        }

        /// <summary>
        /// Gets or sets the q.
        /// </summary>
        /// <value>
        /// The q.
        /// </value>
        public static Spell Q { get; set; }

        /// <summary>
        /// Gets or sets the r.
        /// </summary>
        /// <value>
        /// The r.
        /// </value>
        public static Spell R { get; set; }

        /// <summary>
        /// Gets or sets the w.
        /// </summary>
        /// <value>
        /// The w.
        /// </value>
        public static Spell W { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The on game load.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        public static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Soraka")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.283f, 210, 1100, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);

            CreateMenu();

            PrintChat("Loaded ! Definitely created by Sophie AND NOT CHEWYMOON :3");

            Interrupter2.OnInterruptableTarget += InterrupterOnOnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Game.OnUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        /// <summary>
        ///     Prints to the chat.
        /// </summary>
        /// <param name="msg">The message.</param>
        public static void PrintChat(string msg)
        {
            Game.PrintChat("<font color='#3492EB'>Sophie's Soraka:</font> <font color='#FFFFFF'>" + msg + "</font>");
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The on enemy gapcloser event.
        /// </summary>
        /// <param name="gapcloser">
        ///     The gapcloser.
        /// </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var unit = gapcloser.Sender;

            if (Menu.Item("useQGapcloser").GetValue<bool>() && unit.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(unit, Packets);
            }

            if (Menu.Item("useEGapcloser").GetValue<bool>() && unit.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(unit, Packets);
            }
        }

        /// <summary>
        ///     Automatics the ultimate.
        /// </summary>
        private static void AutoR()
        {
            if (!R.IsReady())
            {
                return;
            }

            if (ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsAlly)
                .Where(x => !x.IsDead)
                .Where(x => !x.IsZombie)
                .Select(x => (int)x.Health / x.MaxHealth * 100)
                .Select(
                    friendHealth => new { friendHealth, health = Menu.Item("autoRPercent").GetValue<Slider>().Value })
                .Where(x => x.friendHealth <= x.health)
                .Select(x => x.friendHealth).Any())
            {
                R.Cast(Packets);
            }
        }

        /// <summary>
        ///     Automatics the W heal.
        /// </summary>
        private static void AutoW()
        {
            if (!W.IsReady())
            {
                return;
            }

            var autoWHealth = Menu.Item("autoWHealth").GetValue<Slider>().Value;
            if (ObjectManager.Player.HealthPercent < autoWHealth)
            {
                return;
            }

            foreach (var friend in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => !x.IsEnemy)
                    .Where(x => !x.IsMe)
                    .Where(friend => W.IsInRange(friend.ServerPosition, W.Range))
                    .Select(friend => new { friend, friendHealth = friend.Health / friend.MaxHealth * 100 })
                    .Select(@t => new { @t, healthPercent = Menu.Item("autoWPercent").GetValue<Slider>().Value })
                    .Where(@t => @t.@t.friendHealth <= @t.healthPercent)
                    .Select(@t => @t.@t.friend))
            {
                W.CastOnUnit(friend, Packets);
            }
        }

        /// <summary>
        ///     The combo.
        /// </summary>
        private static void Combo()
        {
            var useQ = Menu.Item("useQ").GetValue<bool>();
            var useE = Menu.Item("useE").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target, Packets);
            }

            if (useE && E.IsReady())
            {
                E.Cast(target, Packets);
            }
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("Sophies's Soraka", "sSoraka", true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "ssTS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalking
            var orbwalkingMenu = new Menu("Orbwalking", "ssOrbwalking");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkingMenu);
            Menu.AddSubMenu(orbwalkingMenu);

            // Combo
            var comboMenu = new Menu("Combo", "ssCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "ssHarass");
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // Drawing
            var drawingMenu = new Menu("Drawing", "ssDrawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            Menu.AddSubMenu(drawingMenu);

            // Misc
            var miscMenu = new Menu("Misc", "ssMisc");
            miscMenu.AddItem(new MenuItem("packets", "Use Packets").SetValue(true));
            miscMenu.AddItem(new MenuItem("useQGapcloser", "Q on Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("useEGapcloser", "E on Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoW", "Auto use W").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoWPercent", "% Percent").SetValue(new Slider(50, 1)));
            miscMenu.AddItem(new MenuItem("autoWHealth", "My Health Percent").SetValue(new Slider(30, 1)));
            miscMenu.AddItem(new MenuItem("autoR", "Auto use R").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoRPercent", "% Percent").SetValue(new Slider(15, 1)));
            miscMenu.AddItem(new MenuItem("eInterrupt", "Use E to Interrupt").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            Menu.AddToMainMenu();
        }

        /// <summary>
        ///     The on draw event.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("drawQ").GetValue<bool>();
            var drawW = Menu.Item("drawW").GetValue<bool>();
            var drawE = Menu.Item("drawE").GetValue<bool>();

            var p = ObjectManager.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        /// <summary>
        ///     The  on game update event.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void GameOnOnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }

            if (Menu.Item("autoW").GetValue<bool>())
            {
                AutoW();
            }

            if (Menu.Item("autoR").GetValue<bool>())
            {
                AutoR();
            }
        }

        /// <summary>
        ///     The harass.
        /// </summary>
        private static void Harass()
        {
            var useQ = Menu.Item("useQHarass").GetValue<bool>();
            var useE = Menu.Item("useEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target, Packets);
            }

            if (useE && E.IsReady())
            {
                E.Cast(target, Packets);
            }
        }

        /// <summary>
        ///     The on possible to interrupt event.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void InterrupterOnOnPossibleToInterrupt(
            Obj_AI_Hero sender, 
            Interrupter2.InterruptableTargetEventArgs args)
        {
            var unit = sender;
            var spell = args;

            if (Menu.Item("eInterrupt").GetValue<bool>() == false || spell.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            if (!unit.IsValidTarget(E.Range))
            {
                return;
            }

            if (!E.IsReady())
            {
                return;
            }

            E.Cast(unit, Packets);
        }

        #endregion
    }
}