namespace Mid_or_Feed.Champions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;
    using ItemData = LeagueSharp.Common.Data.ItemData;

    /// <summary>
    ///     A plugin for Yasuo.
    /// </summary>
    internal class Yasuo : Plugin
    {
        #region Static Fields

        /// <summary>
        ///     The position before qe
        /// </summary>
        private static Vector3 positionBeforeQe;

        #endregion

        #region Fields

        /// <summary>
        ///     The botrk
        /// </summary>
        public Items.Item Botrk;

        /// <summary>
        ///     The e
        /// </summary>
        public Spell E;

        /// <summary>
        ///     The r
        /// </summary>
        public Spell R;

        /// <summary>
        ///     The w
        /// </summary>
        public Spell W;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Yasuo" /> class.
        /// </summary>
        public Yasuo()
        {
            // Setup Spells
            this.W = new Spell(SpellSlot.W, 400);
            this.E = new Spell(SpellSlot.E, 475);
            this.R = new Spell(SpellSlot.R, 1200);

            this.E.SetTargetted(0.25f, 20);

            // Setup Items
            this.Botrk = ItemData.Blade_of_the_Ruined_King.GetItem();

            PrintChat("Yasuo loaded!");

            Game.OnUpdate += this.Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += this.AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += this.InterrupterOnOnPossibleToInterrupt;
            Drawing.OnDraw += this.DrawingOnOnDraw;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the q.
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        public Spell Q
        {
            get
            {
                var spell = new Spell(SpellSlot.Q);

                switch (this.QCount)
                {
                    case 1:
                    case 2:
                        spell.Range = 520;
                        spell.SetSkillshot(0.35f, 15, 8700, false, SkillshotType.SkillshotLine);
                        break;
                    case 3:
                        spell.Range = 1000;
                        spell.SetSkillshot(0.75f, 90, 1500, false, SkillshotType.SkillshotLine);
                        break;
                }

                return spell;
            }
        }

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
                if (this.Player.HasBuff("yasuoq"))
                {
                    return 2;
                }

                return this.Player.HasBuff("yasuoQ3W") ? 3 : 1;
            }
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
            config.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseEGapclose", "Use E Gapclose").SetValue(true));
            config.AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            config.AddItem(new MenuItem("AutoUseR", "Auto Use R on Enemies").SetValue(new Slider(3, 1, 5)));
        }

        /// <summary>
        ///     Creates the drawing menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            config.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
        }

        /// <summary>
        ///     Gets the combo damage.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override float GetComboDamage(Obj_AI_Hero target)
        {
            float dmg = 0;

            if (this.Q.IsReady())
            {
                dmg += this.Q.GetDamage(target);
            }

            if (this.E.IsReady())
            {
                dmg += this.E.GetDamage(target);
            }

            if (this.R.IsReady() && target.HasBuffOfType(BuffType.Knockup))
            {
                dmg += this.R.GetDamage(target);
            }

            if (this.Botrk.IsReady())
            {
                dmg += (float)this.Player.GetItemDamage(target, Damage.DamageItems.Botrk);
            }

            if (Orbwalking.CanAttack())
            {
                // Include the BotRK passive
                dmg += (float)this.Player.GetAutoAttackDamage(target, true);
            }

            return dmg;
        }

        /// <summary>
        ///     Creates the harass menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            // config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseQEHarass", "Use QE").SetValue(true));
            config.AddItem(new MenuItem("info1", "If QE is on, It will QE enemy"));
            config.AddItem(new MenuItem("info2", "And E back to location you were"));
        }

        /// <summary>
        ///     Creates the item menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void ItemMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseBotRK", "Use BotRK").SetValue(true));
        }

        /// <summary>
        ///     Creates the misc menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void Misc(Menu config)
        {
            var farmingMenu = new Menu("Farming", "yasFarm");

            var lastHitMenu = new Menu("Last Hit", "LastHIt");
            lastHitMenu.AddItem(new MenuItem("UseQLH", "Use Q").SetValue(true));
            lastHitMenu.AddItem(new MenuItem("UseELH", "Use E").SetValue(false));
            lastHitMenu.AddItem(new MenuItem("LastHitOnHarass", "Last Hit in Mixed Mode").SetValue(true));
            lastHitMenu.AddItem(new MenuItem("Use3QLH", "Use 3rd Q for last hitting").SetValue(false));
            farmingMenu.AddSubMenu(lastHitMenu);

            var waveClearMenu = new Menu("Wave/Jungle Clear", "yasWaveClear");
            waveClearMenu.AddItem(new MenuItem("UseQWC", "Use Q").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("UseEWC", "Use E").SetValue(true));
            farmingMenu.AddSubMenu(waveClearMenu);

            config.AddSubMenu(farmingMenu);

            config.AddItem(new MenuItem("UseQInterrupt", "Use Q3 to Interrupt").SetValue(true));
            config.AddItem(new MenuItem("UseQGapclose", "Use Q3 Gapcloser").SetValue(true));
            config.AddItem(new MenuItem("UltDown", "Use R When Falling Down").SetValue(true));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired on when there is an incoming enemy gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (this.QCount != 3 || !this.Q.IsReady() || !this.GetBool("UseQGapclose")
                || !gapcloser.Sender.IsValidTarget())
            {
                return;
            }

            this.Q.Cast(gapcloser.Sender, this.Packets);
        }

        /// <summary>
        ///     Checks the and use r.
        /// </summary>
        private void CheckAndUseR()
        {
            if (!this.R.IsReady())
            {
                return;
            }

            var useR = this.GetBool("UseR");
            var autoREnemies = this.GetValue<Slider>("AutoUseR").Value;
            var useRDown = this.GetBool("UltDown");

            if (!useR)
            {
                return;
            }

            var enemiesKnockedUp =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget(this.R.Range))
                    .Where(x => x.HasBuffOfType(BuffType.Knockup));

            // Prevent Multiple Enumerations
            var enemies = enemiesKnockedUp as IList<Obj_AI_Hero> ?? enemiesKnockedUp.ToList();
            var killableEnemy = enemies.FirstOrDefault(x => this.R.GetDamage(x) > x.Health);
            if (killableEnemy != null)
            {
                if (!this.Q.IsReady())
                {
                    this.R.Cast(this.Packets);
                    return;
                }
            }

            if (autoREnemies > enemies.Count)
            {
                return;
            }

            if (useRDown)
            {
                // Get the lowest end time
                var lowestEndTime =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(this.R.Range))
                        .Where(x => x.HasBuffOfType(BuffType.Knockup))
                        .OrderBy(x => x.Buffs.First(buff => buff.Type == BuffType.Knockup).EndTime)
                        .Select(x => x.Buffs.First(buff => buff.Type == BuffType.Knockup).EndTime)
                        .First();

                var castTime = this.R.Delay + Game.Ping / 2f;

                Utility.DelayAction.Add(
                    (int)((lowestEndTime - Environment.TickCount) - castTime),
                    () => this.R.Cast(this.Packets));
            }
            else
            {
                this.R.Cast(this.Packets);
            }
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private void DoCombo()
        {
            var useQ = this.GetBool("UseQ");
            var useE = this.GetBool("UseE");
            var useEGapclose = this.GetBool("UseEGapclose");

            var target = TargetSelector.GetTarget(this.Q.Range, TargetSelector.DamageType.Physical);

            if (target == null && useEGapclose && this.E.IsReady())
            {
                var targetExt = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);

                if (!targetExt.IsValidTarget())
                {
                    return;
                }

                var bestMinion =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(x => x.IsValidTarget(this.E.Range))
                        .Where(x => x.Distance(targetExt) < this.Player.Distance(targetExt))
                        .OrderByDescending(x => x.Distance(this.Player))
                        .FirstOrDefault();

                if (bestMinion != null)
                {
                    this.E.Cast(bestMinion, this.Packets);
                }

                return;
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            if (useE && useQ && this.Q.IsReady() && this.E.IsReady() && this.E.IsInRange(target, this.E.Range))
            {
                this.E.Cast(target, this.Packets);
                var castTime = (int)((this.Player.Distance(target) / this.E.Speed) + this.E.Delay + Game.Ping / 2f);

                Utility.DelayAction.Add(castTime, delegate { Q.Cast(target, Packets); });

                return;
            }

            if (this.Botrk.IsReady())
            {
                this.Botrk.Cast(target);
            }

            if (this.E.IsReady() && useE)
            {
                this.E.Cast(target, this.Packets);
            }

            if (this.Q.IsReady() && useQ)
            {
                this.Q.Cast(target, this.Packets);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private void DoHarass()
        {
            var useQ = this.GetBool("UseQHarass");
            var useQE = this.GetBool("UseQEHarass");

            var target = TargetSelector.GetTarget(this.Q.Range, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget())
            {
                return;
            }

            // Oh godd
            if (useQE && this.Q.IsReady() && this.E.IsReady() && this.E.IsInRange(target, this.E.Range))
            {
                positionBeforeQe = this.Player.ServerPosition;

                this.E.Cast(target);

                var castTime = (int)((this.Player.Distance(target) / this.E.Speed) + this.E.Delay + Game.Ping / 2f);

                Utility.DelayAction.Add(
                    castTime,
                    delegate
                        {
                            Q.Cast(target, Packets);

                            Utility.DelayAction.Add(
                                (int)(Q.Delay + Game.Ping / 2f),
                                delegate
                                    {
                                        // Find closest minion to last pos
                                        var bestMinion =
                                            ObjectManager.Get<Obj_AI_Minion>()
                                                .Where(x => x.IsValidTarget(E.Range))
                                                .OrderBy(x => x.Distance(positionBeforeQe))
                                                .FirstOrDefault();

                                        if (bestMinion != null)
                                        {
                                            E.Cast(bestMinion, Packets);
                                        }
                                    });
                        });
            }

            if (useQ && this.Q.IsReady())
            {
                this.Q.Cast(target, this.Packets);
            }
        }

        /// <summary>
        ///     Does the last hit.
        /// </summary>
        private void DoLastHit()
        {
            var useQ = this.GetBool("UseQLH");
            var useLastQ = this.GetBool("Use3QLH");
            var useE = this.GetBool("UseELH");
            var lastHitOnHarass = this.GetBool("LastHitOnHarass");

            if (!lastHitOnHarass && this.OrbwalkerMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                return;
            }

            if (!useLastQ && this.QCount == 3)
            {
                return;
            }

            // Prioritize Q over E
            if (useQ && this.Q.IsReady())
            {
                var minion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsValidTarget(this.Q.Range))
                        .FirstOrDefault(x => this.Player.GetSpellDamage(x, SpellSlot.Q) > x.Health);

                if (minion != null)
                {
                    this.Q.Cast(minion);
                }
            }
            else if (useE && this.E.IsReady())
            {
                var minion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsValidTarget(this.Q.Range))
                        .FirstOrDefault(x => this.Player.GetSpellDamage(x, SpellSlot.E) > x.Health);

                if (minion != null)
                {
                    this.E.Cast(minion);
                }
            }
        }

        /// <summary>
        ///     Does the wave clear.
        /// </summary>
        private void DoWaveClear()
        {
            var useQ = this.GetBool("UseQWC");
            var useE = this.GetBool("UseEWC");

            // Q-E Wave Clear
            if (useQ && useE && this.Q.IsReady() && this.E.IsReady())
            {
                var location =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(this.E.Range).Select(x => x.ServerPosition).ToList().To2D(),
                        270,
                        this.E.Range);

                // find nearest minion to that position
                var nearestMinion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsValidTarget(this.E.Range))
                        .OrderBy(x => x.Distance(location.Position))
                        .FirstOrDefault();

                this.E.Cast(nearestMinion, this.Packets);

                var castTimeDelay =
                    (int)((this.Player.Distance(nearestMinion) / this.E.Speed) + this.E.Delay + Game.Ping / 2f);
                Utility.DelayAction.Add(castTimeDelay, () => this.Q.Cast(nearestMinion, this.Packets));
            }

            if (useQ && this.Q.IsReady())
            {
                var location = this.Q.GetLineFarmLocation(MinionManager.GetMinions(this.Q.Range));
                this.Q.Cast(location.Position, this.Packets);
            }

            if (useE && this.E.IsReady())
            {
                var minion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsValidTarget(this.E.Range))
                        .FirstOrDefault(x => this.Player.GetSpellDamage(x, SpellSlot.E) > x.Health);

                if (minion != null)
                {
                    this.E.Cast(minion, this.Packets);
                }
            }
        }

        /// <summary>
        ///     Fired when teh game is drawn..
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = this.GetBool("DrawQ");
            var drawE = this.GetBool("DrawE");
            var drawR = this.GetBool("DrawR");
            var p = this.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, this.Q.Range, this.Q.IsReady() ? Color.Aqua : Color.Red);
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
        private void Game_OnGameUpdate(EventArgs args)
        {
            this.CheckAndUseR();

            switch (this.OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.LastHit:
                    this.DoLastHit();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    this.DoWaveClear();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    this.DoHarass();
                    this.DoLastHit();
                    break;

                case Orbwalking.OrbwalkingMode.Combo:
                    this.DoCombo();
                    break;
            }
        }

        /// <summary>
        ///     Fired on an interruptable target.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private void InterrupterOnOnPossibleToInterrupt(
            Obj_AI_Hero unit,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (this.QCount != 3 || !this.Q.IsReady() || !this.GetBool("UseQInterrupt") || !unit.IsValidTarget()
                || args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            this.Q.Cast(unit, this.Packets);
        }

        #endregion
    }
}