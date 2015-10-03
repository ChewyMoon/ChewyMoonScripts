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
//   The MoonDraven class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MoonDraven
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    /// <summary>
    ///     The MoonDraven class.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    internal class MoonDraven
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        public Spell E { get; set; }

        /// <summary>
        ///     Gets the mana percent.
        /// </summary>
        /// <value>
        ///     The mana percent.
        /// </value>
        public float ManaPercent
        {
            get
            {
                return this.Player.Mana / this.Player.MaxMana * 100;
            }
        }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public Menu Menu { get; set; }

        /// <summary>
        ///     Gets or sets the orbwalker.
        /// </summary>
        /// <value>
        ///     The orbwalker.
        /// </value>
        public Orbwalking.Orbwalker Orbwalker { get; set; }

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        public Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        /// <summary>
        ///     Gets or sets the q.
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        public Spell Q { get; set; }

        /// <summary>
        ///     Gets the q count.
        /// </summary>
        /// <value>
        ///     The q count.
        /// </value>
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
        ///     Gets or sets the q reticles.
        /// </summary>
        /// <value>
        ///     The q reticles.
        /// </value>
        public List<QRecticle> QReticles { get; set; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        public Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the w.
        /// </summary>
        /// <value>
        ///     The w.
        /// </value>
        public Spell W { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the last axe move time.
        /// </summary>
        private int LastAxeMoveTime { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            // Create spells
            this.Q = new Spell(SpellSlot.Q, Orbwalking.GetRealAutoAttackRange(this.Player));
            this.W = new Spell(SpellSlot.W);
            this.E = new Spell(SpellSlot.E, 1050);
            this.R = new Spell(SpellSlot.R);

            this.E.SetSkillshot(0.25f, 130, 1400, false, SkillshotType.SkillshotLine);
            this.R.SetSkillshot(0.4f, 160, 2000, true, SkillshotType.SkillshotLine);

            this.QReticles = new List<QRecticle>();

            this.CreateMenu();

            Game.PrintChat("<font color=\"#7CFC00\"><b>MoonDraven:</b></font> Loaded");

            Obj_AI_Base.OnNewPath += this.Obj_AI_Base_OnNewPath;
            GameObject.OnCreate += this.GameObjectOnOnCreate;
            GameObject.OnDelete += this.GameObjectOnOnDelete;
            AntiGapcloser.OnEnemyGapcloser += this.AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += this.Interrupter2OnOnInterruptableTarget;
            Drawing.OnDraw += this.DrawingOnOnDraw;
            Game.OnUpdate += this.GameOnOnUpdate;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Called on an enemy gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!this.Menu.Item("UseEGapcloser").IsActive() || !this.E.IsReady()
                || !gapcloser.Sender.IsValidTarget(this.E.Range))
            {
                return;
            }

            this.E.Cast(gapcloser.Sender);
        }

        /// <summary>
        ///     Catches the axe.
        /// </summary>
        private void CatchAxe()
        {
            var catchOption = this.Menu.Item("AxeMode").GetValue<StringList>().SelectedIndex;

            if (((catchOption == 0 && this.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                 || (catchOption == 1 && this.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None))
                || catchOption == 2)
            {
                var bestReticle =
                    this.QReticles.Where(
                        x =>
                        x.Object.Position.Distance(Game.CursorPos)
                        < this.Menu.Item("CatchAxeRange").GetValue<Slider>().Value)
                        .OrderBy(x => x.Position.Distance(this.Player.ServerPosition))
                        .ThenBy(x => x.Position.Distance(Game.CursorPos))
                        .ThenBy(x => x.ExpireTime)
                        .FirstOrDefault();

                if (bestReticle != null && bestReticle.Object.Position.Distance(this.Player.ServerPosition) > 100)
                {
                    var eta = 1000 * (this.Player.Distance(bestReticle.Position) / this.Player.MoveSpeed);
                    var expireTime = bestReticle.ExpireTime - Environment.TickCount;

                    if (eta >= expireTime && this.Menu.Item("UseWForQ").IsActive())
                    {
                        this.W.Cast();
                    }

                    if (this.Menu.Item("DontCatchUnderTurret").IsActive())
                    {
                        // If we're under the turret as well as the axe, catch the axe
                        if (this.Player.UnderTurret(true) && bestReticle.Object.Position.UnderTurret(true))
                        {
                            if (this.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
                            {
                                this.Player.IssueOrder(GameObjectOrder.MoveTo, bestReticle.Position);
                            }
                            else
                            {
                                this.Orbwalker.SetOrbwalkingPoint(bestReticle.Position);
                            }
                        }
                        else if (!bestReticle.Position.UnderTurret(true))
                        {
                            if (this.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
                            {
                                this.Player.IssueOrder(GameObjectOrder.MoveTo, bestReticle.Position);
                            }
                            else
                            {
                                this.Orbwalker.SetOrbwalkingPoint(bestReticle.Position);
                            }
                        }
                    }
                    else
                    {
                        if (this.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
                        {
                            this.Player.IssueOrder(GameObjectOrder.MoveTo, bestReticle.Position);
                        }
                        else
                        {
                            this.Orbwalker.SetOrbwalkingPoint(bestReticle.Position);
                        }
                    }
                }
                else
                {
                    this.Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                }
            }
            else
            {
                this.Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
            }
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private void Combo()
        {
            var target = TargetSelector.GetTarget(this.E.Range, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = this.Menu.Item("UseQCombo").IsActive();
            var useW = this.Menu.Item("UseWCombo").IsActive();
            var useE = this.Menu.Item("UseECombo").IsActive();
            var useR = this.Menu.Item("UseRCombo").IsActive();

            if (useQ && this.QCount < this.Menu.Item("MaxAxes").GetValue<Slider>().Value - 1 && this.Q.IsReady()
                && this.Orbwalker.InAutoAttackRange(target) && !this.Player.Spellbook.IsAutoAttacking)
            {
                this.Q.Cast();
            }

            if (useW && this.W.IsReady()
                && this.ManaPercent > this.Menu.Item("UseWManaPercent").GetValue<Slider>().Value)
            {
                if (this.Menu.Item("UseWSetting").IsActive())
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
                HeroManager.Enemies.Where(x => x.IsValidTarget(2000))
                    .FirstOrDefault(
                        x =>
                        this.Player.GetSpellDamage(x, SpellSlot.R) * 2 > x.Health
                        && (!this.Orbwalker.InAutoAttackRange(x) || this.Player.CountEnemiesInRange(this.E.Range) > 2));

            if (killableTarget != null)
            {
                this.R.Cast(killableTarget);
            }
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            this.Menu = new Menu("MoonDraven", "cmMoonDraven", true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "ts");
            TargetSelector.AddToMenu(tsMenu);
            this.Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkMenu = new Menu("Orbwalker", "orbwalker");
            this.Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            this.Menu.AddSubMenu(orbwalkMenu);

            // Combo
            var comboMenu = new Menu("Combo", "combo");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            this.Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "harass");
            harassMenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            harassMenu.AddItem(
                new MenuItem("UseHarassToggle", "Harass! (Toggle)").SetValue(new KeyBind(84, KeyBindType.Toggle)));
            this.Menu.AddSubMenu(harassMenu);

            // Lane Clear
            var laneClearMenu = new Menu("Wave Clear", "waveclear");
            laneClearMenu.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("UseWWaveClear", "Use W").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("UseEWaveClear", "Use E").SetValue(false));
            laneClearMenu.AddItem(new MenuItem("WaveClearManaPercent", "Mana Percent").SetValue(new Slider(50)));
            this.Menu.AddSubMenu(laneClearMenu);

            // Axe Menu
            var axeMenu = new Menu("Axe Settings", "axeSetting");
            axeMenu.AddItem(
                new MenuItem("AxeMode", "Catch Axe on Mode:").SetValue(
                    new StringList(new[] { "Combo", "Any", "Always" }, 2)));
            axeMenu.AddItem(new MenuItem("CatchAxeRange", "Catch Axe Range").SetValue(new Slider(800, 120, 1500)));
            axeMenu.AddItem(new MenuItem("MaxAxes", "Maximum Axes").SetValue(new Slider(2, 1, 3)));
            axeMenu.AddItem(new MenuItem("UseWForQ", "Use W if Axe too far").SetValue(true));
            axeMenu.AddItem(new MenuItem("DontCatchUnderTurret", "Don't Catch Axe Under Turret").SetValue(true));
            this.Menu.AddSubMenu(axeMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "draw");
            drawMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawAxeLocation", "Draw Axe Location").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawAxeRange", "Draw Axe Catch Range").SetValue(true));
            this.Menu.AddSubMenu(drawMenu);

            // Misc Menu
            var miscMenu = new Menu("Misc", "misc");
            miscMenu.AddItem(new MenuItem("UseWSetting", "Use W Instantly(When Available)").SetValue(false));
            miscMenu.AddItem(new MenuItem("UseEGapcloser", "Use E on Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("UseEInterrupt", "Use E to Interrupt").SetValue(true));
            miscMenu.AddItem(new MenuItem("UseWManaPercent", "Use W Mana Percent").SetValue(new Slider(50)));
            miscMenu.AddItem(new MenuItem("UseWSlow", "Use W if Slowed").SetValue(true));
            this.Menu.AddSubMenu(miscMenu);

            this.Menu.AddToMainMenu();
        }

        /// <summary>
        ///     Called when the game draws itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawE = this.Menu.Item("DrawE").IsActive();
            var drawAxeLocation = this.Menu.Item("DrawAxeLocation").IsActive();
            var drawAxeRange = this.Menu.Item("DrawAxeRange").IsActive();

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
                        x.Position.Distance(Game.CursorPos) < this.Menu.Item("CatchAxeRange").GetValue<Slider>().Value)
                        .OrderBy(x => x.Position.Distance(this.Player.ServerPosition))
                        .ThenBy(x => x.Position.Distance(Game.CursorPos))
                        .FirstOrDefault();

                if (bestAxe != null)
                {
                    Render.Circle.DrawCircle(bestAxe.Position, 120, Color.LimeGreen);
                }

                foreach (var axe in
                    this.QReticles.Where(x => x.Object.NetworkId != (bestAxe == null ? 0 : bestAxe.Object.NetworkId)))
                {
                    Render.Circle.DrawCircle(axe.Position, 120, Color.Yellow);
                }
            }

            if (drawAxeRange)
            {
                Render.Circle.DrawCircle(
                    Game.CursorPos,
                    this.Menu.Item("CatchAxeRange").GetValue<Slider>().Value,
                    Color.DodgerBlue);
            }
        }

        /// <summary>
        ///     Called when a game object is created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                return;
            }

            this.QReticles.Add(new QRecticle(sender, Environment.TickCount + 1800));
            Utility.DelayAction.Add(1800, () => this.QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId));
        }

        /// <summary>
        ///     Called when a game object is deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameObjectOnOnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                return;
            }

            this.QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId);
        }

        /// <summary>
        ///     Called when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameOnOnUpdate(EventArgs args)
        {
            this.QReticles.RemoveAll(x => x.Object.IsDead);

            if (this.W.IsReady() && this.Menu.Item("UseWSlow").IsActive() && this.Player.HasBuffOfType(BuffType.Slow))
            {
                this.W.Cast();
            }

            switch (this.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    this.Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    this.LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    this.Combo();
                    break;
            }

            if (this.Menu.Item("UseHarassToggle").IsActive())
            {
                this.Harass();
            }
        }

        /// <summary>
        ///     Harasses the enemy.
        /// </summary>
        private void Harass()
        {
            var target = TargetSelector.GetTarget(this.E.Range, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (this.Menu.Item("UseEHarass").IsActive() && this.E.IsReady())
            {
                this.E.Cast(target);
            }
        }

        /// <summary>
        ///     Interrupts an interruptable target.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private void Interrupter2OnOnInterruptableTarget(
            Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!this.Menu.Item("UseEInterrupt").IsActive() || !this.E.IsReady() || !sender.IsValidTarget(this.E.Range))
            {
                return;
            }

            if (args.DangerLevel == Interrupter2.DangerLevel.Medium || args.DangerLevel == Interrupter2.DangerLevel.High)
            {
                this.E.Cast(sender);
            }
        }

        /// <summary>
        ///     Clears the lane of minions.
        /// </summary>
        private void LaneClear()
        {
            var useQ = this.Menu.Item("UseQWaveClear").IsActive();
            var useW = this.Menu.Item("UseWWaveClear").IsActive();
            var useE = this.Menu.Item("UseEWaveClear").IsActive();

            if (this.ManaPercent < this.Menu.Item("WaveClearManaPercent").GetValue<Slider>().Value)
            {
                return;
            }

            if (useQ && this.QCount < this.Menu.Item("MaxAxes").GetValue<Slider>().Value - 1 && this.Q.IsReady()
                && this.Orbwalker.GetTarget() is Obj_AI_Minion && !this.Player.Spellbook.IsAutoAttacking)
            {
                this.Q.Cast();
            }

            if (useW && this.W.IsReady()
                && this.ManaPercent > this.Menu.Item("UseWManaPercent").GetValue<Slider>().Value)
            {
                if (this.Menu.Item("UseWSetting").IsActive())
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

            var bestLocation = this.E.GetLineFarmLocation(MinionManager.GetMinions(this.E.Range));

            if (bestLocation.MinionsHit > 1)
            {
                this.E.Cast(bestLocation.Position);
            }
        }

        /// <summary>
        ///     Fired when the OnNewPath event is called.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectNewPathEventArgs" /> instance containing the event data.</param>
        private void Obj_AI_Base_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (!sender.IsMe || this.QReticles.Any(x => x.Position.Distance(args.Path.LastOrDefault()) < 110))
            {
                return;
            }

            this.CatchAxe();
        }

        #endregion

        /// <summary>
        ///     A represenation of a Q circle on Draven.
        /// </summary>
        internal class QRecticle
        {
            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="QRecticle" /> class.
            /// </summary>
            /// <param name="rectice">The rectice.</param>
            /// <param name="expireTime">The expire time.</param>
            public QRecticle(GameObject rectice, int expireTime)
            {
                this.Object = rectice;
                this.ExpireTime = expireTime;
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     Gets or sets the expire time.
            /// </summary>
            /// <value>
            ///     The expire time.
            /// </value>
            public int ExpireTime { get; set; }

            /// <summary>
            ///     Gets or sets the object.
            /// </summary>
            /// <value>
            ///     The object.
            /// </value>
            public GameObject Object { get; set; }

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

            #endregion
        }
    }
}