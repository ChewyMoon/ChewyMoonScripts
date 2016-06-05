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
    using System.Reflection;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The sophies soraka.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
        Justification = "Reviewed. Suppression is OK here.")]
    internal class SophiesSoraka
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        public static Spell E { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public static Menu Menu { get; set; }

        /// <summary>
        ///     Gets or sets the orbwalker.
        /// </summary>
        /// <value>
        ///     The orbwalker.
        /// </value>
        public static Orbwalking.Orbwalker Orbwalker { get; set; }

        /// <summary>
        ///     Gets a value indicating whether to use packets.
        /// </summary>
        public static bool Packets
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets or sets the q.
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        public static Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        public static Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the w.
        /// </summary>
        /// <value>
        ///     The w.
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

            Q = new Spell(SpellSlot.Q, 750);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R);
            
            Q.SetSkillshot(0.3f, 125, 1750, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.4f, 70f, 1750, false, SkillshotType.SkillshotCircle);

            CreateMenu();

            PrintChat("loaded!");

            Interrupter2.OnInterruptableTarget += InterrupterOnOnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Game.OnUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            Orbwalking.BeforeAttack += OrbwalkingOnBeforeAttack;
        }

        /// <summary>
        ///     Prints to the chat.
        /// </summary>
        /// <param name="msg">The message.</param>
        public static void PrintChat(string msg)
        {
            Game.PrintChat("<font color='#F778A1'><b>Sophie's Soraka:</b></font> <font color='#FFFFFF'>" + msg + "</font>");
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The on enemy gapcloser event.
        /// </summary>
        /// <param name="gapcloser">
        ///     The gapcloser.
        /// </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
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
                    .Any(
                        x =>
                        x.IsAlly && x.IsValidTarget(float.MaxValue, false)
                        && x.HealthPercent < Menu.Item("autoRPercent").GetValue<Slider>().Value))
            {
                R.Cast();
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

            var dontWInFountain = Menu.Item("DontWInFountain").GetValue<bool>();
            if (dontWInFountain && ObjectManager.Player.InFountain())
            {
                return;
            }

            var healthPercent = Menu.Item("autoWPercent").GetValue<Slider>().Value;

            var canidates = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(W.Range, false) && x.IsAlly && x.HealthPercent < healthPercent);
            var wMode = Menu.Item("HealingPriority").GetValue<StringList>().SelectedValue;

            switch (wMode)
            {
                case "Most AD":
                    canidates = canidates.OrderByDescending(x => x.TotalAttackDamage);
                    break;
                case "Most AP":
                    canidates = canidates.OrderByDescending(x => x.TotalMagicalDamage);
                    break;
                case "Least Health":
                    canidates = canidates.OrderBy(x => x.Health);
                    break;
                case "Least Health (Prioritize Squishies)":
                    canidates = canidates.OrderBy(x => x.Health).ThenBy(x => x.MaxHealth);
                    break;
            }

            var target = dontWInFountain ? canidates.FirstOrDefault(x => !x.InFountain()) : canidates.FirstOrDefault();

            if (target != null)
            {
                W.CastOnUnit(target);
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
            var comboMenu = new Menu("Combo Settings", "ssCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass Settings", "ssHarass");
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("HarassMana", "Harass Mana Percent").SetValue(new Slider(50)));
            harassMenu.AddItem(
                new MenuItem("HarassToggle", "Harass! (toggle)").SetValue(new KeyBind(84, KeyBindType.Toggle)));
            Menu.AddSubMenu(harassMenu);

            // Healing
            var healingMenu = new Menu("Healing Settings", "ssHeal");

            var wMenu = new Menu("W Settings", "WSettings");
            wMenu.AddItem(new MenuItem("autoW", "Use W").SetValue(true));
            wMenu.AddItem(new MenuItem("autoWPercent", "Ally Health Percent").SetValue(new Slider(50, 1)));
            wMenu.AddItem(new MenuItem("autoWHealth", "My Health Percent").SetValue(new Slider(30, 1)));
            wMenu.AddItem(new MenuItem("DontWInFountain", "Dont W in Fountain").SetValue(true));
            wMenu.AddItem(
                new MenuItem("HealingPriority", "Healing Priority").SetValue(
                    new StringList(
                        new[] { "Most AD", "Most AP", "Least Health", "Least Health (Prioritize Squishies)" }, 
                        3)));
            healingMenu.AddSubMenu(wMenu);

            var rMenu = new Menu("R Settings", "RSettings");
            rMenu.AddItem(new MenuItem("autoR", "Use R").SetValue(true));
            rMenu.AddItem(new MenuItem("autoRPercent", "% Percent").SetValue(new Slider(15, 1)));
            healingMenu.AddSubMenu(rMenu);

            Menu.AddSubMenu(healingMenu);

            // Drawing
            var drawingMenu = new Menu("Drawing Settings", "ssDrawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            Menu.AddSubMenu(drawingMenu);

            // Misc
            var miscMenu = new Menu("Miscellaneous Settings", "ssMisc");
            miscMenu.AddItem(new MenuItem("useQGapcloser", "Q on Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("useEGapcloser", "E on Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("eInterrupt", "Use E to Interrupt").SetValue(true));
            miscMenu.AddItem(new MenuItem("AttackMinions", "Attack Minions").SetValue(false));
            miscMenu.AddItem(new MenuItem("AttackChampions", "Attack Champions").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            Menu.AddItem(new MenuItem("Seperator", string.Empty));
            Menu.AddItem(
                new MenuItem("Version", "Sophie's Soraka " + Assembly.GetExecutingAssembly().GetName().Version)
                    .SetFontStyle(FontStyle.Bold, new SharpDX.Color(247, 120, 161, 255)));
            Menu.AddItem(new MenuItem("Author", "Made by ChewyMoon!").SetFontStyle(FontStyle.Bold));
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

            if (Menu.Item("HarassToggle").IsActive()
                && ObjectManager.Player.Mana > Menu.Item("HarassMana").GetValue<Slider>().Value)
            {
                Harass();
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

        /// <summary>
        ///     Called before the orbwalker attacks a unit.
        /// </summary>
        /// <param name="args">The <see cref="Orbwalking.BeforeAttackEventArgs" /> instance containing the event data.</param>
        private static void OrbwalkingOnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target.IsValid<Obj_AI_Minion>() && !Menu.Item("AttackMinions").IsActive() && ObjectManager.Player.CountAlliesInRange(1200) > 0)
            {
                args.Process = false;
            }

            if (args.Target.IsValid<Obj_AI_Hero>() &&  !Menu.Item("AttackChampions").GetValue<bool>() && ObjectManager.Player.CountAlliesInRange(1000) > 0)
            {
                args.Process = false;
            }
        }

        #endregion
    }
}
