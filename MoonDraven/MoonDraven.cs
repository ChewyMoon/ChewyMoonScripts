// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoonDraven.cs" company="ChewyMoon">
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
//   Main Class for MoonDraven logic.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace MoonDraven
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows.Forms;

    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Events;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using LeagueSharp.SDK.Core.Utils;
    using LeagueSharp.SDK.Core.Wrappers;

    using SharpDX;

    using Color = System.Drawing.Color;
    using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

    /// <summary>
    ///     Main Class for MoonDraven logic.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
        Justification = "Reviewed. Suppression is OK here.")]
    internal class MoonDraven
    {
        /// <summary>
        ///     Gets or sets the E spell.
        /// </summary>
        /// <value>
        ///     The E Spell.
        /// </value>
        public Spell E { get; set; }

        /// <summary>
        ///     Gets or sets the Q spell.
        /// </summary>
        /// <value>
        ///     The Q spell.
        /// </value>
        public Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the Q reticles.
        /// </summary>
        /// <value>
        ///     The Q reticles.
        /// </value>
        public List<QRecticle> QReticles { get; set; }

        /// <summary>
        ///     Gets or sets the R spell.
        /// </summary>
        /// <value>
        ///     The R spell.
        /// </value>
        public Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the W spell.
        /// </summary>
        /// <value>
        ///     The W spell.
        /// </value>
        public Spell W { get; set; }

        /// <summary>
        ///     Gets or sets the orbwalker.
        /// </summary>
        public Orbwalker Orbwalker { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        public Menu Menu { get; set; }

        /// <summary>
        ///     Gets the player.
        /// </summary>
        public Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        /// <summary>
        ///     Gets the q count.
        /// </summary>
        public int QCount
        {
            get
            {
                return (this.Player.HasBuff("dravenspinningattack")
                            ? this.Player.Buffs.First(x => x.Name == "dravenspinningattack").Count
                            : 0) + this.QReticles.Count;
            }
        }

        /// <summary>
        ///     Gets the mana percent.
        /// </summary>
        public float ManaPercent
        {
            get
            {
                return this.Player.Mana / this.Player.MaxMana * 100;
            }
        }

        /// <summary>
        ///     The load.
        /// </summary>
        public void Load()
        {
            // Initialize list
            this.QReticles = new List<QRecticle>();

            // Create spells
            this.Q = new Spell(SpellSlot.Q, this.Player.GetRealAutoAttackRange());
            this.W = new Spell(SpellSlot.W);
            this.E = new Spell(SpellSlot.E, 0x44C);
            this.R = new Spell(SpellSlot.R);

            this.E.SetSkillshot(0.25f, 0x82, 0x578, false, SkillshotType.SkillshotLine);
            this.R.SetSkillshot(0.4f, 0xA0, 0x7D0, true, SkillshotType.SkillshotLine);

            this.CreateMenu();

            Game.PrintChat("<font color=\"#7CFC00\"><b>MoonDraven:</b></font> Loaded");

            GameObject.OnCreate += this.GameObjectOnOnCreate;
            GameObject.OnDelete += this.GameObjectOnOnDelete;
            Gapcloser.OnGapCloser += this.AntiGapcloserOnOnEnemyGapcloser;
            InterruptableSpell.OnInterruptableTarget += this.Interrupter2OnOnInterruptableTarget;
            Drawing.OnDraw += this.DrawingOnOnDraw;
            Game.OnUpdate += this.GameOnOnUpdate;
        }

        /// <summary>
        /// The drawing on on draw.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawE = this.Menu["Drawing"]["DrawE"].GetValue<MenuBool>().Value;
            var drawAxeLocation = this.Menu["Drawing"]["DrawAxeLocation"].GetValue<MenuBool>().Value;
            var drawAxeRange = this.Menu["Drawing"]["DrawAxeRange"].GetValue<MenuBool>().Value;

            if (drawE)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, 
                    this.E.Range, 
                    this.E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawAxeLocation)
            {
                var bestAxe =
                    this.QReticles.Where(
                        x =>
                        x.Position.Distance(Game.CursorPos)
                        < this.Menu["AxeSettings"]["CatchAxeRange"].GetValue<MenuSlider>().Value)
                        .OrderBy(x => x.Position.Distance(this.Player.ServerPosition))
                        .ThenBy(x => x.Position.Distance(Game.CursorPos))
                        .FirstOrDefault();

                if (bestAxe != null)
                {
                    Render.Circle.DrawCircle(bestAxe.Position, 0x78, Color.LimeGreen);
                }

                foreach (var axe in
                    this.QReticles.Where(x => x.Object.NetworkId != (bestAxe == null ? 0 : bestAxe.Object.NetworkId)))
                {
                    Render.Circle.DrawCircle(axe.Position, 0x78, Color.Yellow);
                }
            }

            if (drawAxeRange)
            {
                Render.Circle.DrawCircle(
                    Game.CursorPos, 
                    this.Menu["AxeSettings"]["CatchAxeRange"].GetValue<MenuSlider>().Value, 
                    Color.DodgerBlue);
            }
        }

        /// <summary>
        /// The interrupter 2 on on interruptable target.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="interruptableTargetEventArgs">
        /// The interruptable target event args.
        /// </param>
        private void Interrupter2OnOnInterruptableTarget(
            object o, 
            InterruptableSpell.InterruptableTargetEventArgs interruptableTargetEventArgs)
        {
            var sender = interruptableTargetEventArgs.Sender;
            if (!this.Menu["Misc"]["UseEInterrupt"].GetValue<MenuBool>().Value || !this.E.IsReady()
                || !sender.IsValidTarget(this.E.Range))
            {
                return;
            }

            if (interruptableTargetEventArgs.DangerLevel == DangerLevel.Medium
                || interruptableTargetEventArgs.DangerLevel == DangerLevel.High)
            {
                this.E.Cast(sender);
            }
        }

        /// <summary>
        /// The anti gapcloser on on enemy gapcloser.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="gapCloserEventArgs">
        /// The gap closer event args.
        /// </param>
        private void AntiGapcloserOnOnEnemyGapcloser(object sender, Gapcloser.GapCloserEventArgs gapCloserEventArgs)
        {
            if (!this.Menu["Misc"]["UseEGapcloser"].GetValue<MenuBool>().Value || !this.E.IsReady()
                || !gapCloserEventArgs.Sender.IsValidTarget(this.E.Range))
            {
                return;
            }

            this.E.Cast(gapCloserEventArgs.Sender);
        }

        /// <summary>
        /// The game object on on delete.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private void GameObjectOnOnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                return;
            }

            this.QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId);
        }

        /// <summary>
        /// The game object on on create.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                return;
            }

            this.QReticles.Add(new QRecticle(sender, Environment.TickCount + 0x708));
            DelayAction.Add(0x708, () => this.QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId));
        }

        /// <summary>
        /// The game on on update.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private void GameOnOnUpdate(EventArgs args)
        {
            var catchOptionObject = this.Menu["AxeSettings"]["AxeMode"].GetValue<MenuList<string>>();
            var catchOption = catchOptionObject.Index;

            if ((catchOption == 0 && Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk)
                || (catchOption == 1 && Orbwalker.ActiveMode != OrbwalkerMode.None) || catchOption == 2)
            {
                var bestReticle =
                    this.QReticles.Where(
                        x =>
                        x.Object.Position.Distance(Game.CursorPos)
                        < this.Menu["AxeSettings"]["CatchAxeRange"].GetValue<MenuSlider>().Value)
                        .OrderBy(x => x.Position.Distance(this.Player.ServerPosition))
                        .ThenBy(x => x.Position.Distance(Game.CursorPos))
                        .FirstOrDefault();

                if (bestReticle != null && bestReticle.Object.Position.Distance(this.Player.ServerPosition) > 0x6E)
                {
                    var eta = 1000 * (this.Player.Distance(bestReticle.Position) / this.Player.MoveSpeed);
                    var expireTime = bestReticle.ExpireTime - Environment.TickCount;

                    if (eta >= expireTime && this.Menu["AxeSettings"]["UseWForQ"].GetValue<MenuBool>().Value)
                    {
                        this.W.Cast();
                    }

                    if (this.Menu["AxeSettings"]["DontCatchUnderTurret"].GetValue<MenuBool>().Value)
                    {
                        // If we're under the turret as well as the axe, catch the axe
                        if (this.Player.IsUnderTurret(true) && bestReticle.Object.Position.IsUnderTurret(true))
                        {
                            if (Orbwalker.ActiveMode == OrbwalkerMode.None)
                            {
                                this.Player.IssueOrder(GameObjectOrder.MoveTo, bestReticle.Position);
                            }
                            else
                            {
                                Orbwalker.OrbwalkPosition = bestReticle.Position;
                            }
                        }
                        else if (!bestReticle.Position.IsUnderTurret(true))
                        {
                            // Catch axe if not under turret
                            if (Orbwalker.ActiveMode == OrbwalkerMode.None)
                            {
                                this.Player.IssueOrder(GameObjectOrder.MoveTo, bestReticle.Position);
                            }
                            else
                            {
                                Orbwalker.OrbwalkPosition = bestReticle.Position;
                            }
                        }
                    }
                    else
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.None)
                        {
                            this.Player.IssueOrder(GameObjectOrder.MoveTo, bestReticle.Position);
                        }
                        else
                        {
                            Orbwalker.OrbwalkPosition = bestReticle.Position;
                        }
                    }
                }
                else
                {
                    Orbwalker.OrbwalkPosition = Game.CursorPos;
                }
            }
            else
            {
                Orbwalker.OrbwalkPosition = Game.CursorPos;
            }

            if (this.W.IsReady() && this.Menu["Misc"]["UseWSlow"].GetValue<MenuBool>().Value
                && this.Player.HasBuffOfType(BuffType.Slow))
            {
                this.W.Cast();
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Hybrid:
                    this.Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    this.LaneClear();
                    break;
                case OrbwalkerMode.Orbwalk:
                    this.Combo();
                    break;
            }

            if (this.Menu["Harass"]["UseHarassToggle"].GetValue<MenuBool>().Value)
            {
                this.Harass();
            }
        }

        /// <summary>
        ///     The combo.
        /// </summary>
        private void Combo()
        {
            var target = TargetSelector.GetTarget(this.E.Range);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = this.Menu["Combo"]["UseQCombo"].GetValue<MenuBool>().Value;
            var useW = this.Menu["Combo"]["UseWCombo"].GetValue<MenuBool>().Value;
            var useE = this.Menu["Combo"]["UseECombo"].GetValue<MenuBool>().Value;
            var useR = this.Menu["Combo"]["UseRCombo"].GetValue<MenuBool>().Value;

            if (useQ && this.QCount < this.Menu["AxeSettings"]["MaxAxes"].GetValue<MenuSlider>().Value - 1
                && this.Q.IsReady() && target.InAutoAttackRange() && !this.Player.Spellbook.IsAutoAttacking)
            {
                this.Q.Cast();
            }

            if (useW && this.W.IsReady()
                && this.ManaPercent > this.Menu["Misc"]["UseWManaPercent"].GetValue<MenuSlider>().Value)
            {
                if (this.Menu["Misc"]["UseWSetting"].GetValue<MenuBool>().Value)
                {
                    this.W.Cast();
                }
                else
                {
                    if (!this.Player.HasBuff("dravenfurybuff"))
                    {
                        this.W.Cast();
                    }
                }
            }

            if (useE && this.E.IsReady())
            {
                this.E.Cast(target);
            }

            if (!useR || !this.R.IsReady())
            {
                return;
            }

            // Patented Advanced Algorithms D321987
            var killableTarget =
                GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(0x7D0))
                    .FirstOrDefault(
                        x =>
                        this.Player.GetSpellDamage(x, SpellSlot.R) * 2 > x.Health
                        && (!x.InAutoAttackRange()
                            || GameObjects.EnemyHeroes.Count(y => y.IsValidTarget(this.E.Range)) > 2));

            if (killableTarget != null)
            {
                this.R.Cast(killableTarget);
            }
        }

        /// <summary>
        ///     The lane clear.
        /// </summary>
        private void LaneClear()
        {
            var useQ = this.Menu["WaveClear"]["UseQWaveClear"].GetValue<MenuBool>().Value;
            var useW = this.Menu["WaveClear"]["UseWWaveClear"].GetValue<MenuBool>().Value;
            var useE = this.Menu["WaveClear"]["UseEWaveClear"].GetValue<MenuBool>().Value;

            if (this.ManaPercent < this.Menu["WaveClear"]["WaveClearManaPercent"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            if (useQ && this.QCount < this.Menu["AxeSettings"]["MaxAxes"].GetValue<MenuSlider>().Value - 1
                && this.Q.IsReady() && Orbwalker.GetTarget(Orbwalker.ActiveMode) is Obj_AI_Minion
                && !this.Player.Spellbook.IsAutoAttacking)
            {
                this.Q.Cast();
            }

            if (useW && this.W.IsReady()
                && this.ManaPercent > this.Menu["Misc"]["UseWManaPercent"].GetValue<MenuSlider>().Value)
            {
                if (this.Menu["Misc"]["UseWSetting"].GetValue<MenuBool>().Value)
                {
                    this.W.Cast();
                }
                else
                {
                    if (!this.Player.HasBuff("dravenfurybuff"))
                    {
                        this.W.Cast();
                    }
                }
            }

            if (!useE || !this.E.IsReady())
            {
                return;
            }

            var bestLocation =
                this.E.GetLineFarmLocation(
                    GameObjects.EnemyMinions.Where(x => x.IsValidTarget(this.E.Range)).Cast<Obj_AI_Base>().ToList());

            if (bestLocation.MinionsHit > 1)
            {
                this.E.Cast(bestLocation.Position);
            }
        }

        /// <summary>
        ///     The harass.
        /// </summary>
        private void Harass()
        {
            var target = TargetSelector.GetTarget(this.E.Range);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (this.Menu["Harass"]["UseEHarass"].GetValue<MenuBool>().Value && this.E.IsReady())
            {
                this.E.Cast(target);
            }
        }

        /// <summary>
        ///     The create menu.
        /// </summary>
        private void CreateMenu()
        {
            this.Menu = new Menu("cmMoonDraven", "Moon Draven", true);

            // Combo
            var comboMenu = new Menu("Combo", "Combo");
            comboMenu.Add(new MenuBool("UseQCombo", "Use Q", true));
            comboMenu.Add(new MenuBool("UseWCombo", "Use W", true));
            comboMenu.Add(new MenuBool("UseECombo", "Use E", true));
            comboMenu.Add(new MenuBool("UseRCombo", "Use R", true));
            this.Menu.Add(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "Harass");
            harassMenu.Add(new MenuBool("UseEHarass", "Use E", true));
            harassMenu.Add(new MenuKeyBind("UseHarassToggle", "Harass! (Toggle)", Keys.T, KeyBindType.Toggle));
            this.Menu.Add(harassMenu);

            // Lane Clear
            var laneClearMenu = new Menu("WaveClear", "Wave Clear");
            laneClearMenu.Add(new MenuBool("UseQWaveClear", "Use Q", true));
            laneClearMenu.Add(new MenuBool("UseWWaveClear", "Use W", true));
            laneClearMenu.Add(new MenuBool("UseEWaveClear", "Use E"));
            laneClearMenu.Add(new MenuSlider("WaveClearManaPercent", "Mana Percent", 0x32));
            this.Menu.Add(laneClearMenu);

            // Axe Menu
            var axeMenu = new Menu("AxeSettings", "Axe Settings");
            axeMenu.Add(new MenuList<string>("AxeMode", "Catch Axe on Mode:", new[] { "Combo", "Any", "Always" }));
            axeMenu.Add(new MenuSlider("CatchAxeRange", "Catch Axe Range", 0x320, 0x78, 0x5DC));
            axeMenu.Add(new MenuSlider("MaxAxes", "Maximum Axes", 2, 1, 3));
            axeMenu.Add(new MenuBool("UseWForQ", "Use W if Axe too far", true));
            axeMenu.Add(new MenuBool("DontCatchUnderTurret", "Don't Catch Axe Under Turret", true));
            this.Menu.Add(axeMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Draw");
            drawMenu.Add(new MenuBool("DrawE", "Draw E", true));
            drawMenu.Add(new MenuBool("DrawAxeLocation", "Draw Axe Location", true));
            drawMenu.Add(new MenuBool("DrawAxeRange", "Draw Axe Catch Range", true));
            this.Menu.Add(drawMenu);

            // Misc Menu
            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.Add(new MenuBool("UseWSetting", "Use W Instantly(When Available)"));
            miscMenu.Add(new MenuBool("UseEGapcloser", "Use E on Gapcloser", true));
            miscMenu.Add(new MenuBool("UseEInterrupt", "Use E to Interrupt", true));
            miscMenu.Add(new MenuSlider("UseWManaPercent", "Use W Mana Percent", 0x32));
            miscMenu.Add(new MenuBool("UseWSlow", "Use W if Slowed", true));
            this.Menu.Add(miscMenu);

            this.Menu.Attach();
        }

        /// <summary>
        ///     Represents dravens axe marker.
        /// </summary>
        internal class QRecticle
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QRecticle"/> class.
            /// </summary>
            /// <param name="rectice">
            /// The rectice.
            /// </param>
            /// <param name="expireTime">
            /// The expire time.
            /// </param>
            public QRecticle(GameObject rectice, int expireTime)
            {
                this.Object = rectice;
                this.ExpireTime = expireTime;
            }

            /// <summary>
            ///     Gets or sets the object.
            /// </summary>
            /// <value>
            ///     The object.
            /// </value>
            public GameObject Object { get; set; }

            /// <summary>
            ///     Gets or sets the expire time.
            /// </summary>
            /// <value>
            ///     The expire time.
            /// </value>
            public int ExpireTime { get; set; }

            /// <summary>
            ///     Gets the position.
            /// </summary>
            /// <value>
            ///     The position.
            /// </value>
            public Vector3 Position
            {
                get
                {
                    return this.Object.Position;
                }
            }
        }
    }
}