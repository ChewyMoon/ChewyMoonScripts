namespace Mid_or_Feed.Champions
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     A plugin for Malzahar.
    /// </summary>
    internal class Malzahar : Plugin
    {
        #region Fields

        /// <summary>
        ///     The spell list
        /// </summary>
        public readonly List<Spell> SpellList;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Malzahar" /> class.
        /// </summary>
        public Malzahar()
        {
            // Create spells
            var q = new Spell(SpellSlot.Q, 900);
            var w = new Spell(SpellSlot.W, 800);
            var e = new Spell(SpellSlot.E, 650);
            var r = new Spell(SpellSlot.R, 700);

            q.SetSkillshot(0.5f, 100, float.MaxValue, false, SkillshotType.SkillshotLine);
            w.SetSkillshot(0.5f, 240, 20, false, SkillshotType.SkillshotCircle);

            this.SpellList = new List<Spell> { q, w, e, r };

            PrintChat("Malzahar loaded.");

            Game.OnUpdate += this.Game_OnGameUpdate;
            Drawing.OnDraw += this.DrawingOnOnDraw;
            AntiGapcloser.OnEnemyGapcloser += this.AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += this.InterrupterOnOnPossibleToInterrupt;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the combo menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseR", "Use R").SetValue(true));
        }

        /// <summary>
        ///     Creates the drawing menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ", "DrawQ").SetValue(true));
            config.AddItem(new MenuItem("DrawW", "DrawW").SetValue(true));
            config.AddItem(new MenuItem("DrawE", "DrawE").SetValue(true));
            config.AddItem(new MenuItem("DrawR", "DrawR").SetValue(true));
        }

        /// <summary>
        ///     Gets the combo damage.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override float GetComboDamage(Obj_AI_Hero target)
        {
            return this.SpellList.Where(x => x.IsReady()).Sum(spell => spell.GetDamage(target));
        }

        /// <summary>
        ///     Creates the harass menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
        }

        /// <summary>
        ///     Creates the misc menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Misc(Menu config)
        {
            config.AddItem(new MenuItem("InterruptQ", "Use Q to Interrupt").SetValue(true));
            config.AddItem(new MenuItem("GapcloserQ", "Use Q on Gapcloser").SetValue(true));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired on an incoming enemy gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget())
            {
                return;
            }

            if (!this.GetBool("GapcloserQ"))
            {
                return;
            }

            this.GetSpell(this.SpellList, SpellSlot.Q).Cast(gapcloser.Sender);
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);

            if (this.Player.IsCastingInterruptableSpell() || this.Player.IsChannelingImportantSpell())
            {
                return;
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            foreach (
                var spell in this.SpellList.Where(x => x.IsReady()).Where(x => this.GetBool("Use" + x.Slot.ToString())))
            {
                if (spell.Slot != SpellSlot.R)
                {
                    spell.Cast(target, this.Packets);
                }
                else if (spell.Slot == SpellSlot.R && this.ShouldUseR(target))
                {
                    spell.CastOnUnit(target, this.Packets);
                }
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            foreach (var spell in
                this.SpellList.Where(x => x.IsReady())
                    .Where(x => x.Slot != SpellSlot.R)
                    .Where(x => this.GetBool("Use" + x.Slot.ToString() + "Harass")))
            {
                spell.Cast(target, this.Packets);
            }
        }

        /// <summary>
        ///     Fired when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void DrawingOnOnDraw(EventArgs args)
        {
            foreach (var spell in this.SpellList.Where(x => this.GetBool("Draw" + x.Slot.ToString())))
            {
                Render.Circle.DrawCircle(this.Player.Position, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        /// <summary>
        ///     Fired when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Game_OnGameUpdate(EventArgs args)
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
        ///     Fired when a target can be interrupted.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private void InterrupterOnOnPossibleToInterrupt(
            Obj_AI_Hero unit,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!unit.IsValidTarget())
            {
                return;
            }

            if (args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            if (!this.GetBool("InterruptQ"))
            {
                return;
            }

            this.GetSpell(this.SpellList, SpellSlot.Q).Cast(unit);
        }

        /// <summary>
        ///     Shoulds the use r.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        private bool ShouldUseR(Obj_AI_Base target)
        {
            return this.Player.GetSpellDamage(target, SpellSlot.R) > target.Health;
        }

        #endregion
    }
}