#region



#endregion

namespace Mid_or_Feed.Champions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    /// <summary>
    ///     A Lux plugin.
    /// </summary>
    internal class Lux : Plugin
    {
        #region Static Fields

        /// <summary>
        ///     The e
        /// </summary>
        public static Spell E;

        /// <summary>
        ///     The e game object
        /// </summary>
        public static GameObject EGameObject;

        /// <summary>
        ///     The q
        /// </summary>
        public static Spell Q;

        /// <summary>
        ///     The r
        /// </summary>
        public static Spell R;

        /// <summary>
        ///     The w
        /// </summary>
        public static Spell W;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Lux" /> class.
        /// </summary>
        public Lux()
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            Q.SetSkillshot(0.5f, 80, 1200, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 150, 1200, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 275, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.75f, 190, 3000, false, SkillshotType.SkillshotLine);

            GameObject.OnCreate += delegate(GameObject sender, EventArgs args)
                {
                    //Noticed a different E game object name while looking through Kurisu's oracle, credits to him.
                    if (sender.Name.Contains("LuxLightstrike_tar_green")
                        || sender.Name.Contains("LuxLightstrike_tar_red"))
                    {
                        EGameObject = sender;
                    }
                };

            GameObject.OnDelete += delegate(GameObject sender, EventArgs args)
                {
                    //Noticed a different E game object name while looking through Kurisu's oracle, credits to him.
                    if (sender.Name.Contains("LuxLightstrike_tar_"))
                    {
                        EGameObject = null;
                    }
                };

            Game.OnUpdate += this.GameOnOnGameUpdate;
            Drawing.OnDraw += this.DrawingOnOnDraw;
            Obj_AI_Base.OnTeleport += this.ObjAiHeroOnOnTeleport;
            AntiGapcloser.OnEnemyGapcloser += this.AntiGapcloserOnOnEnemyGapcloser;

            PrintChat("Lux loaded!");
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether the e is activated.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the e is activated.; otherwise, <c>false</c>.
        /// </value>
        public static bool EActivated
        {
            get
            {
                return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1 || EGameObject != null;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Determines whether the specified hero has the passive.
        /// </summary>
        /// <param name="hero">The hero.</param>
        /// <returns></returns>
        public static bool HasPassive(Obj_AI_Hero hero)
        {
            return hero.HasBuff("luxilluminatingfraulein");
        }

        /// <summary>
        ///     Comboes the specified combo menu.
        /// </summary>
        /// <param name="comboMenu">The combo menu.</param>
        public override void Combo(Menu comboMenu)
        {
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useQSlowed", "Only use Q if slowed by E").SetValue(false));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(false));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(false));
            comboMenu.AddItem(new MenuItem("useRKillable", "R only if Killable").SetValue(true));

            comboMenu.Item("useRKillable").ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
                {
                    if (args.GetNewValue<bool>() && GetBool("useR"))
                    {
                        Menu.Item("useR").SetValue(false);
                    }
                };

            comboMenu.Item("useR").ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
                {
                    if (args.GetNewValue<bool>() && GetBool("useRKillable"))
                    {
                        Menu.Item("useRKillable").SetValue(false);
                    }
                };
        }

        /// <summary>
        ///     Drawingses the specified drawing menu.
        /// </summary>
        /// <param name="drawingMenu">The drawing menu.</param>
        public override void Drawings(Menu drawingMenu)
        {
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));
        }

        /// <summary>
        ///     Gets the combo damage.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;
            var p = this.Player;

            if (Q.IsReady())
            {
                dmg += p.GetDamageSpell(target, SpellSlot.Q).CalculatedDamage;
            }

            if (E.IsReady())
            {
                dmg += p.GetDamageSpell(target, SpellSlot.E).CalculatedDamage;
            }

            if (R.IsReady())
            {
                dmg += p.GetDamageSpell(target, SpellSlot.R).CalculatedDamage;
            }

            return (float)dmg;
        }

        /// <summary>
        ///     Harasses the specified harass menu.
        /// </summary>
        /// <param name="harassMenu">The harass menu.</param>
        public override void Harass(Menu harassMenu)
        {
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));
        }

        /// <summary>
        ///     Miscs the specified misc menu.
        /// </summary>
        /// <param name="miscMenu">The misc menu.</param>
        public override void Misc(Menu miscMenu)
        {
            // Buff Steal Menu
            var buffStealMenu = new Menu("Buff Stealing", "mofLuxBFF");
            buffStealMenu.AddItem(new MenuItem("StealEnemyRed", "Steal Enemy Red Buff").SetValue(true));
            buffStealMenu.AddItem(new MenuItem("StealEnemyBlue", "Steal Enemy Blue").SetValue(true));
            buffStealMenu.AddItem(new MenuItem("StealAllyBlue", "Steal Ally Blue Buff").SetValue(false));
            buffStealMenu.AddItem(new MenuItem("StealAllyRed", "Steal Ally Red Buff").SetValue(false));
            buffStealMenu.AddItem(
                new MenuItem("AutoSteal", "Steal(Toggle)").SetValue(new KeyBind(84, KeyBindType.Press, true)));
            buffStealMenu.AddItem(new MenuItem("KeySteal", "Steal(Press)").SetValue(new KeyBind(90, KeyBindType.Press)));
            miscMenu.AddSubMenu(buffStealMenu);

            miscMenu.AddItem(new MenuItem("rKS", "Use R to KS").SetValue(true));
            miscMenu.AddItem(new MenuItem("rKSRecall", "KS enemies b'ing in FOW").SetValue(true));
            miscMenu.AddItem(new MenuItem("qGapcloser", "Q on Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoW", "Auto use W").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoWPercent", "% Health").SetValue(new Slider(15, 1)));
        }

        /// <summary>
        ///     Creates the wave clear menu.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public override void WaveClear(Menu config)
        {
            config.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseEWaveClear", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRWaveClear", "Use R").SetValue(false));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired when there is an incoming gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!this.GetBool("qGapcloser"))
            {
                return;
            }

            Q.Cast(gapcloser.Sender, this.Packets);
        }

        /// <summary>
        ///     Automatics the w.
        /// </summary>
        private void AutoW()
        {
            if (!W.IsReady() || this.Player.IsRecalling())
            {
                return;
            }

            foreach (
                var ally in from ally in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly).Where(x => !x.IsDead)
                            let allyPercent = ally.Health / ally.MaxHealth * 100
                            let healthPercent = this.GetValue<Slider>("autoWPercent").Value
                            where healthPercent >= allyPercent
                            select ally)
            {
                W.Cast(ally, this.Packets, true);
                return;
            }
        }

        /// <summary>
        ///     Casts the e.
        /// </summary>
        /// <param name="target">The target.</param>
        private void CastE(Obj_AI_Hero target)
        {
            //TODO: Remake this
            if (EActivated)
            {
                if (
                    !ObjectManager.Get<Obj_AI_Hero>()
                         .Where(x => x.IsEnemy)
                         .Where(x => !x.IsDead)
                         .Where(x => x.IsValidTarget())
                         .Any(enemy => enemy.Distance(EGameObject.Position) < E.Width))
                {
                    return;
                }

                var isInAaRange = this.Player.Distance(target) <= Orbwalking.GetRealAutoAttackRange(this.Player);

                if (isInAaRange && !HasPassive(target))
                {
                    E.Cast(this.Packets);
                }

                // Pop E if the target is out of AA range
                if (!isInAaRange)
                {
                    E.Cast(this.Packets);
                }
            }
            else
            {
                E.Cast(target, this.Packets);
            }
        }

        /// <summary>
        ///     Casts the q.
        /// </summary>
        /// <param name="target">The target.</param>
        private void CastQ(Obj_AI_Base target)
        {
            var input = Q.GetPrediction(target);
            var col = Q.GetCollision(this.Player.ServerPosition.To2D(), new List<Vector2> { input.CastPosition.To2D() });
            var minions = col.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

            if (minions <= 1)
            {
                Q.Cast(input.CastPosition, this.Packets);
            }
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            var useQ = this.GetBool("useQ");
            var useQSlowed = this.GetBool("useQSlowed");
            var useW = this.GetBool("useW");
            var useE = this.GetBool("useE");
            var useR = this.GetBool("useR");
            var useRKillable = this.GetBool("useRKillable");

            if (useQ && !HasPassive(target) && Q.IsReady() && !useQSlowed)
            {
                this.CastQ(target);
            }

            if (useQSlowed)
            {
                if (target.HasBuffOfType(BuffType.Slow) && EActivated && target.Distance(EGameObject.Position) < E.Width)
                {
                    Q.Cast(target, this.Packets);
                }

                // Cast Q if E is not up
                if (!E.IsReady() && !EActivated && !HasPassive(target))
                {
                    Q.Cast(target, this.Packets);
                }
            }

            if (useW && W.IsReady())
            {
                W.Cast(Game.CursorPos, this.Packets);
            }

            if (useE && E.IsReady())
            {
                this.CastE(target);
            }

            if (useR && R.IsReady())
            {
                R.Cast(target, this.Packets, true);
            }

            if (!useRKillable)
            {
                return;
            }
            var killable = this.Player.GetSpellDamage(target, SpellSlot.R) > target.Health;
            if (killable && R.IsReady())
            {
                R.Cast(target, this.Packets, true);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            var useQ = this.GetBool("useQHarass");
            var useE = this.GetBool("useEHarass");

            if (useQ && !HasPassive(target) && Q.IsReady())
            {
                this.CastQ(target);
            }

            if (!useE || !E.IsReady())
            {
                return;
            }
            this.CastE(target);
        }

        /// <summary>
        ///     Does the lane clear.
        /// </summary>
        private void DoLaneClear()
        {
            var useQ = this.GetBool("UseQWaveClear");
            var useE = this.GetBool("UseEWaveClear");
            var useR = this.GetBool("UseRWaveClear");

            if (useQ && Q.IsReady())
            {
                var farmLoc = Q.GetLineFarmLocation(MinionManager.GetMinions(Q.Range + this.Player.BoundingRadius));

                if (farmLoc.MinionsHit > 1)
                {
                    Q.Cast(farmLoc.Position);
                }
            }

            if (useE && E.IsReady() && !EActivated)
            {
                var farmLoc = E.GetCircularFarmLocation(MinionManager.GetMinions(E.Range + this.Player.BoundingRadius));

                if (farmLoc.MinionsHit > 1)
                {
                    E.Cast(farmLoc.Position);
                }
            }
            else if (EActivated)
            {
                E.Cast();
            }

            if (useR && R.IsReady())
            {
                var farmLoc = R.GetLineFarmLocation(MinionManager.GetMinions(R.Range + this.Player.BoundingRadius));

                if (farmLoc.MinionsHit > 1)
                {
                    R.Cast(farmLoc.Position);
                }
            }
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
            var drawR = this.GetBool("drawR");

            var p = this.Player.Position;

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

            if (drawR)
            {
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
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

                case Orbwalking.OrbwalkingMode.LaneClear:
                    this.DoLaneClear();
                    break;
            }

            if (this.GetBool("autoW"))
            {
                this.AutoW();
            }

            if (this.GetBool("rKS"))
            {
                this.ItsKillSecure();
            }

            // Steal buffs, become challenger ggez
            if (this.GetValue<KeyBind>("AutoSteal").Active || this.GetValue<KeyBind>("KeySteal").Active)
            {
                this.StealBlue(this.GetBool("StealAllyBlue"), this.GetBool("StealEnemyBlue"));
            }

            if (this.GetValue<KeyBind>("AutoSteal").Active || this.GetValue<KeyBind>("KeySteal").Active)
            {
                this.StealRed(this.GetBool("StealAllyRed"), this.GetBool("StealEnemyRed"));
            }
        }

        /// <summary>
        ///     Steals kills.
        /// </summary>
        private void ItsKillSecure()
        {
            if (!R.IsReady())
            {
                return;
            }

            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget())
                    .Where(x => !x.IsZombie)
                    .Where(x => !x.IsDead)
                    .Where(enemy => this.Player.GetDamageSpell(enemy, SpellSlot.R).CalculatedDamage > enemy.Health))
            {
                R.Cast(enemy, this.Packets);
                return;
            }
        }

        /// <summary>
        ///     Fired when a unit teleports.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectTeleportEventArgs" /> instance containing the event data.</param>
        private void ObjAiHeroOnOnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            if (!this.GetBool("rKSRecall"))
            {
                return;
            }

            var decoded = Packet.S2C.Teleport.Decoded(sender, args);
            var hero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(decoded.UnitNetworkId);

            if (hero.IsAlly || decoded.Type != Packet.S2C.Teleport.Type.Recall
                || decoded.Status != Packet.S2C.Teleport.Status.Start)
            {
                return;
            }

            var rDamage = this.Player.GetSpellDamage(hero, SpellSlot.R);
            if (rDamage > hero.Health)
            {
                R.Cast(hero);
            }
        }

        /// <summary>
        ///     Steals the blue.
        /// </summary>
        /// <param name="stealFriendly">if set to <c>true</c> [steal friendly].</param>
        /// <param name="stealEnemy">if set to <c>true</c> [steal enemy].</param>
        private void StealBlue(bool stealFriendly, bool stealEnemy)
        {
            if (!R.IsReady())
            {
                return;
            }

            var blueBuff =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(x => x.BaseSkinName == "SRU_Blue")
                    .Where(x => this.Player.GetSpellDamage(x, SpellSlot.R) > x.Health)
                    .FirstOrDefault(x => (stealFriendly && x.IsAlly) || (stealEnemy && x.IsEnemy));

            if (blueBuff != null)
            {
                R.Cast(blueBuff, this.Packets);
            }
        }

        /// <summary>
        ///     Steals the red.
        /// </summary>
        /// <param name="stealFriendly">if set to <c>true</c> [steal friendly].</param>
        /// <param name="stealEnemy">if set to <c>true</c> [steal enemy].</param>
        private void StealRed(bool stealFriendly, bool stealEnemy)
        {
            if (!R.IsReady())
            {
                return;
            }

            var redBuff =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(x => x.BaseSkinName == "SRU_Red")
                    .Where(x => this.Player.GetSpellDamage(x, SpellSlot.R) > x.Health)
                    .FirstOrDefault(x => (stealFriendly && x.IsAlly) || (stealEnemy && x.IsEnemy));

            if (redBuff != null)
            {
                R.Cast(redBuff, this.Packets);
            }
        }

        #endregion
    }
}