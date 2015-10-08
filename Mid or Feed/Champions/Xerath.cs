namespace Mid_or_Feed.Champions
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     A plugin for xerath.
    /// </summary>
    internal class Xerath : Plugin
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Xerath" /> class.
        /// </summary>
        public Xerath()
        {
            this.Q = new Spell(SpellSlot.Q, 800);
            this.W = new Spell(SpellSlot.W, 1000);
            this.E = new Spell(SpellSlot.E, 1050);
            this.R = new Spell(SpellSlot.R, 3200);

            this.Q.SetSkillshot(0.6f, 100, int.MaxValue, false, SkillshotType.SkillshotLine);
            this.Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 800, 1600, 1.45f);
            this.W.SetSkillshot(0.7f, 200, int.MaxValue, false, SkillshotType.SkillshotCircle);
            this.E.SetSkillshot(0.26f, 60, 1400, true, SkillshotType.SkillshotLine);
            this.R.SetSkillshot(0.7f, 120, int.MaxValue, false, SkillshotType.SkillshotCircle);

            // Add items to spell list
            this.SpellList = new List<Spell> { this.E, this.Q, this.W, this.R };

            PrintChat("Xerath loaded!");

            Game.OnUpdate += this.GameOnOnUpdate;
            Drawing.OnDraw += this.DrawingOnOnDraw;
            AntiGapcloser.OnEnemyGapcloser += this.AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += this.Interrupter2_OnInterruptableTarget;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        private Spell E { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is channeling r.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is channeling r; otherwise, <c>false</c>.
        /// </value>
        private bool IsChannelingR
        {
            get
            {
                return this.Player.HasBuff("xerathrshots");
            }
        }

        /// <summary>
        ///     Gets or sets the q.
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        private Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        private Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the spell list.
        /// </summary>
        /// <value>
        ///     The spell list.
        /// </value>
        private List<Spell> SpellList { get; set; }

        /// <summary>
        ///     Gets or sets the w.
        /// </summary>
        /// <value>
        ///     The w.
        /// </value>
        private Spell W { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the combo menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
        }

        /// <summary>
        ///     Creates the drawing menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            config.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            config.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            config.AddItem(new MenuItem("DrawRMinimap", "Draw R on Minimap").SetValue(true));
        }

        /// <summary>
        ///     Creates the harass menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRHarass", "Use R").SetValue(true));
        }

        /// <summary>
        ///     Creates the misc menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Misc(Menu config)
        {
            // Suggestions are nice!
            config.AddItem(new MenuItem("UseRKS", "KS with R").SetValue(true));
            config.AddItem(new MenuItem("UseEGapcloser", "E Gapcloser").SetValue(true));
            config.AddItem(new MenuItem("UseWGapcloser", "W Gapcloser").SetValue(true));
            config.AddItem(new MenuItem("UseEInterrupt", "Interrupt Spells with E").SetValue(true));
        }

        /// <summary>
        ///     Creates the wave clear menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void WaveClear(Menu config)
        {
            config.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWWaveClear", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEWaveClear", "Use E").SetValue(true));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired when there is an incoming gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var unit = gapcloser.Sender;
            if (!unit.IsValidTarget())
            {
                return;
            }

            if (this.E.IsReady() && this.E.IsInRange(unit) && this.GetBool("UseEGapcloser"))
            {
                this.E.Cast(unit);
            }
            else if (this.W.IsReady() && this.W.IsInRange(unit) && this.GetBool("UseWGapcloser"))
            {
                this.W.Cast(unit);
            }
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(1600, TargetSelector.DamageType.Magical);

            foreach (var spell in
                this.SpellList.Where(
                    x => x.IsReady() && x.Slot != SpellSlot.R && this.GetBool("Use" + x.Slot + "Combo")))
            {
                if (spell.Slot == SpellSlot.Q)
                {
                    if (!this.Q.IsCharging)
                    {
                        this.Q.StartCharging();
                    }
                    else
                    {
                        this.Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    }
                }
                else
                {
                    spell.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                }
            }

            if (!this.R.IsReady() || !this.GetBool("UseRCombo"))
            {
                return;
            }

            foreach (var enemy in
                HeroManager.Enemies.Where(x => x.IsValidTarget(this.R.Range))
                    .Where(
                        enemy =>
                        (!this.SpellList.Where(x => x.IsReady()).Any(x => x.IsInRange(enemy) && x.IsKillable(enemy)))
                        || this.IsChannelingR))
            {
                this.R.Cast(enemy);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(1600, TargetSelector.DamageType.Magical);

            foreach (var spell in
                this.SpellList.Where(
                    x => x.IsReady() && x.Slot != SpellSlot.R && this.GetBool("Use" + x.Slot + "Harass")))
            {
                if (spell.Slot == SpellSlot.Q)
                {
                    if (!this.Q.IsCharging)
                    {
                        this.Q.StartCharging();
                    }
                    else
                    {
                        this.Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    }
                }
                else
                {
                    spell.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                }
            }
        }

        /// <summary>
        ///     Does the lane clear.
        /// </summary>
        private void DoLaneClear()
        {
            foreach (var spell in
                this.SpellList.Where(
                    x => x.IsReady() && x.Slot != SpellSlot.R && this.GetBool("Use" + x.Slot + "WaveClear")))
            {
                switch (spell.Slot)
                {
                    case SpellSlot.Q:
                        {
                            var farmLocation = this.Q.GetLineFarmLocation(MinionManager.GetMinions(1600));

                            if (farmLocation.MinionsHit <= 1)
                            {
                                continue;
                            }

                            if (!this.Q.IsCharging)
                            {
                                this.Q.StartCharging();
                            }
                            else
                            {
                                this.Q.Cast(farmLocation.Position);
                            }
                        }
                        break;
                    case SpellSlot.W:
                        {
                            var farmLocation = this.W.GetCircularFarmLocation(MinionManager.GetMinions(this.W.Range));

                            if (farmLocation.MinionsHit > 1)
                            {
                                this.W.Cast(farmLocation.Position);
                            }
                        }
                        break;
                    case SpellSlot.E:
                        {
                            var farmLocation = this.E.GetLineFarmLocation(MinionManager.GetMinions(this.E.Range));

                            if (farmLocation.MinionsHit > 1)
                            {
                                this.E.Cast(farmLocation.Position);
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        ///     Fired when the game is drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = this.GetBool("DrawQ");
            var drawW = this.GetBool("DrawW");
            var drawE = this.GetBool("DrawE");
            var drawR = this.GetBool("DrawR");
            var p = this.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, this.Q.Range, this.Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(p, this.W.Range, this.W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, this.E.Range, this.E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(p, this.R.Range, this.R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        /// <summary>
        ///     Fired when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameOnOnUpdate(EventArgs args)
        {
            if (this.GetBool("UseRKS"))
            {
                this.KillSecure();
            }

            switch (this.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LaneClear:
                    this.DoLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    this.DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    this.DoCombo();
                    break;
            }
        }

        /// <summary>
        ///     Fired when a target can be interrupted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private void Interrupter2_OnInterruptableTarget(
            Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsValidTarget(this.E.Range) || !this.E.IsReady()
                || args.DangerLevel != Interrupter2.DangerLevel.High || !this.GetBool("UseEInterrupt"))
            {
                return;
            }

            this.E.Cast(sender);
        }

        /// <summary>
        ///     Steals kills.
        /// </summary>
        private void KillSecure()
        {
            var bestTarget =
                HeroManager.Enemies.Where(
                    x => x.IsValidTarget(this.R.Range) && this.Player.GetSpellDamage(x, SpellSlot.R) * 3 > x.Health)
                    .OrderBy(x => x.Distance(this.Player))
                    .FirstOrDefault();

            if (!bestTarget.IsValidTarget())
            {
                return;
            }

            this.R.Cast(bestTarget);
        }

        #endregion
    }
}