#region



#endregion

namespace Mid_or_Feed.Champions
{
    using System;
    using System.Drawing;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     A plugin for Ahri.
    /// </summary>
    internal class Ahri : Plugin
    {
        #region Fields

        //TODO: Implment some type of Ult logic.

        /// <summary>
        ///     The e
        /// </summary>
        public Spell E;

        /// <summary>
        ///     The q
        /// </summary>
        public Spell Q;

        /// <summary>
        ///     The w
        /// </summary>
        public Spell W;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Ahri" /> class.
        /// </summary>
        public Ahri()
        {
            this.Q = new Spell(SpellSlot.Q, 1000);
            this.W = new Spell(SpellSlot.W, 800);
            this.E = new Spell(SpellSlot.E, 1000);

            this.Q.SetSkillshot(0.25f, 100, 2500, false, SkillshotType.SkillshotLine);
            this.E.SetSkillshot(0.25f, 60, 1500, true, SkillshotType.SkillshotLine);

            Game.OnUpdate += this.GameOnOnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += this.AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += this.InterrupterOnOnPossibleToInterrupt;
            Drawing.OnDraw += this.DrawingOnOnDraw;

            PrintChat("Ahri loaded.");
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the combo menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            config.AddItem(new MenuItem("useE", "Use E").SetValue(true));
        }

        /// <summary>
        ///     Creates the drawing menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            config.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
        }

        /// <summary>
        ///     Gets the combo damage.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;

            if (this.Q.IsReady())
            {
                dmg += this.Player.GetSpellDamage(target, SpellSlot.Q)
                       + this.Player.GetSpellDamage(target, SpellSlot.Q, 1);
            }

            if (this.W.IsReady())
            {
                dmg += this.Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (this.E.IsReady())
            {
                dmg += this.Player.GetSpellDamage(target, SpellSlot.E);
                dmg += dmg * 0.2;
            }

            return (float)dmg;
        }

        /// <summary>
        ///     Creates the harass menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
        }

        /// <summary>
        ///     Creates the misc menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Misc(Menu config)
        {
            config.AddItem(new MenuItem("gapcloseE", "E on Gapcloser", true).SetValue(true));
            config.AddItem(new MenuItem("interruptE", "E to Interrupt").SetValue(true));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired on an enemy gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!this.GetBool("gapcloseE"))
            {
                return;
            }

            this.E.Cast(gapcloser.Sender, this.Packets);
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            var useQ = this.GetBool("useQ");
            var useW = this.GetBool("useW");
            var useE = this.GetBool("useE");

            if (useE && this.E.IsReady())
            {
                this.E.Cast(target);
            }

            if (useQ && this.Q.IsReady())
            {
                this.Q.Cast(target);
            }

            if (useW && this.W.IsReady() && this.W.IsInRange(target.ServerPosition, this.W.Range))
            {
                this.W.Cast(this.Packets);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (!this.GetBool("useQHarass") || !this.Q.IsReady())
            {
                return;
            }

            this.Q.Cast(target, this.Packets);
        }

        /// <summary>
        ///     Fired when the game is drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = this.GetBool("drawQ");
            var drawW = this.GetBool("drawW");
            var drawE = this.GetBool("drawE");
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
        }

        /// <summary>
        ///     Fired when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameOnOnGameUpdate(EventArgs args)
        {
            switch (this.OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    this.DoHarass();
                    break;

                case Orbwalking.OrbwalkingMode.Combo:
                    this.DoCombo();
                    break;
            }
        }

        /// <summary>
        ///     Fired on an interruptable target.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private void InterrupterOnOnPossibleToInterrupt(
            Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!this.GetBool("interruptE") || args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            this.E.Cast(sender, this.Packets);
        }

        #endregion
    }
}