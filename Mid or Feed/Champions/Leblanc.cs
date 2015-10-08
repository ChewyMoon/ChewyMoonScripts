#region



#endregion

namespace Mid_or_Feed.Champions
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     A plugin for Leblanc.
    /// </summary>
    internal class Leblanc : Plugin
    {
        #region Static Fields

        /// <summary>
        ///     The e
        /// </summary>
        public static Spell E;

        /// <summary>
        ///     The q
        /// </summary>
        public static Spell Q;

        /// <summary>
        ///     The r
        /// </summary>
        public static Spell R;

        /// <summary>
        ///     The spell list
        /// </summary>
        public static List<Spell> SpellList;

        /// <summary>
        ///     The w
        /// </summary>
        public static Spell W;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Leblanc" /> class.
        /// </summary>
        public Leblanc()
        {
            Q = new Spell(SpellSlot.Q, 720);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R);

            W.SetSkillshot(0.5f, 220, 1500, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 70, 1600, true, SkillshotType.SkillshotLine);

            // Populate spell list
            SpellList = new List<Spell> { Q, W, E, R };

            // Setup Events
            Game.OnUpdate += this.GameOnOnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += this.AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += this.Interrupter_OnPossibleToInterrupt;
            Drawing.OnDraw += this.Drawing_OnDraw;
            //Obj_AI_Base.OnIssueOrder += ObjAiBaseOnOnIssueOrder;

            PrintChat("LeBlanc loaded!");
        }

        #endregion

        #region Enums

        /// <summary>
        ///     Represents what spell the R wil cast.
        /// </summary>
        public enum RSpell
        {
            /// <summary>
            ///     The q
            /// </summary>
            Q,

            /// <summary>
            ///     The w
            /// </summary>
            W,

            /// <summary>
            ///     The e
            /// </summary>
            E,

            /// <summary>
            ///     Unknown.
            /// </summary>
            Unknown
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether this instance has a valid clone.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has a valid clone; otherwise, <c>false</c>.
        /// </value>
        public bool HasValidClone
        {
            get
            {
                var clone = this.Player.Pet as Obj_AI_Base;
                return clone != null && clone.IsValid && !clone.IsDead;
            }
        }

        public bool RActivated
        {
            get
            {
                return this.Player.Spellbook.GetSpell(SpellSlot.R).Name.ToLower() == "leblancslidereturnm";
            }
        }

        /// <summary>
        ///     Gets the r status.
        /// </summary>
        /// <value>
        ///     The r status.
        /// </value>
        public RSpell RStatus
        {
            get
            {
                var name = this.Player.Spellbook.GetSpell(SpellSlot.R).Name;

                switch (name)
                {
                    case "LeblancChaosOrbM":
                        return RSpell.Q;
                    case "LeblancSlideM":
                        return RSpell.W;
                    case "LeblancSoulShackleM":
                        return RSpell.E;
                }

                return RSpell.Unknown;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the w is activated.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the w is activated; otherwise, <c>false</c>.
        /// </value>
        public bool WActivated
        {
            get
            {
                return this.Player.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslidereturn";
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Comboes the specified combo menu.
        /// </summary>
        /// <param name="comboMenu">The combo menu.</param>
        public override void Combo(Menu comboMenu)
        {
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useWBack", "W/R back when enemy dead").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("DontDoubleE", "Dont Double Chain").SetValue(true));
        }

        /// <summary>
        ///     Drawingses the specified drawing menu.
        /// </summary>
        /// <param name="drawingMenu">The drawing menu.</param>
        public override void Drawings(Menu drawingMenu)
        {
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
        }

        /// <summary>
        ///     Gets the combo damage.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;

            if (Q.IsReady())
            {
                dmg += this.Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (W.IsReady())
            {
                dmg += this.Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (E.IsReady())
            {
                dmg += this.Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (R.IsReady())
            {
                dmg += this.Player.GetSpellDamage(target, SpellSlot.R);
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
            harassMenu.AddItem(new MenuItem("useWHarass", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("useWBackHarass", "W Back").SetValue(true));
        }

        /// <summary>
        ///     Determines whether the specified target has the e buff.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public bool HasEBuff(Obj_AI_Base target)
        {
            return target.HasBuff("LeblancSoulShackle", true) || target.HasBuff("LeblancSoulShackleM", true);
        }

        /// <summary>
        ///     Determines whether the specified target has the q buff.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public bool HasQBuff(Obj_AI_Hero target)
        {
            return target.HasBuff("LeblancChaosOrb") || target.HasBuff("LeblancChaosOrbM");
        }

        /// <summary>
        ///     Miscs the specified misc menu.
        /// </summary>
        /// <param name="miscMenu">The misc menu.</param>
        public override void Misc(Menu miscMenu)
        {
            var fleeMenu = new Menu("Flee", "mofLbFlee");
            fleeMenu.AddItem(new MenuItem("Flee.UseW", "Use W/R").SetValue(true));
            fleeMenu.AddItem(new MenuItem("Flee.DoubleW", "Double Jump(W + R)").SetValue(true));
            fleeMenu.AddItem(new MenuItem("Flee.UseE", "Use E").SetValue(true));
            miscMenu.AddSubMenu(fleeMenu);

            miscMenu.AddItem(new MenuItem("eGapcloser", "E Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("eInterrupt", "E to Interrupt").SetValue(true));
            //miscMenu.AddItem(
            //  new MenuItem("CloneLogic", "Clone Logic").SetValue(
            //  new StringList(new[] {"Follow", "To Target"})));
            // miscMenu.AddItem(new MenuItem("FollowDelay", "Clone Follow Delay(MS)").SetValue(new Slider(300, 0, 1000)));
            miscMenu.AddItem(new MenuItem("Flee", "Flee!").SetValue(new KeyBind('z', KeyBindType.Press)));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fired when there is an incoming gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!this.GetBool("eGapcloser"))
            {
                return;
            }

            if (E.IsReady())
            {
                E.Cast(gapcloser.Sender, this.Packets);
            }
            else if (this.RStatus == RSpell.E && R.IsReady())
            {
                R.Cast(gapcloser.Sender, this.Packets);
            }
        }

        /// <summary>
        ///     Does the clone logic.
        /// </summary>
        private void DoCloneLogic()
        {
            var clone = this.Player.Pet as Obj_AI_Base;

            // Don't have clone or not valid
            if (!this.HasValidClone)
            {
                return;
            }

            switch (this.Menu.Item("CloneLogic").GetValue<StringList>().SelectedValue)
            {
                // Follow
                case "Follow":
                    var delay = this.Menu.Item("FollowDelay").GetValue<Slider>().Value;
                    var moveTo = this.Player.GetWaypoints().Count < 1
                                     ? this.Player.ServerPosition
                                     : this.Player.GetWaypoints().FirstOrDefault().To3D();

                    Utility.DelayAction.Add(
                        delay,
                        () =>
                            {
                                if (!this.HasValidClone)
                                {
                                    return;
                                }

                                if (clone != null)
                                {
                                    clone.IssueOrder(GameObjectOrder.MovePet, moveTo);
                                }
                            });
                    break;

                // To enemy
                case "To Enemy":
                    var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                    if (clone != null && target != null)
                    {
                        clone.IssueOrder(GameObjectOrder.AutoAttackPet, target);
                    }
                    break;
            }
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private void DoCombo()
        {
            // Prioritize target in Q range to use q
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical)
                         ?? TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var dontDoubleE = this.GetBool("DontDoubleE");

            if (target == null)
            {
                return;
            }

            foreach (var spell in SpellList.Where(x => x.IsReady()).Where(spell => this.GetBool("use" + spell.Slot)))
            {
                if (spell.Slot == SpellSlot.Q)
                {
                    Q.CastOnUnit(target, this.Packets);
                }

                if (spell.Slot == SpellSlot.W && !this.WActivated)
                {
                    W.Cast(target, this.Packets);
                }

                if (spell.Slot == SpellSlot.E)
                {
                    if (dontDoubleE && !this.HasEBuff(target))
                    {
                        E.Cast(target, this.Packets);
                    }
                    else
                    {
                        E.Cast(target, this.Packets);
                    }
                }

                if (spell.Slot == SpellSlot.R)
                {
                    if (this.RStatus == RSpell.Q)
                    {
                        R.CastOnUnit(target, this.Packets);
                    }

                    if (this.RStatus == RSpell.W && !this.RActivated)
                    {
                        R.Cast(target, this.Packets);
                    }

                    else if (this.RStatus == RSpell.E)
                    {
                        if (dontDoubleE && !this.HasEBuff(target))
                        {
                            R.Cast(target, this.Packets);
                        }
                        else
                        {
                            R.Cast(target, this.Packets);
                        }
                    }
                }
            }

            if (!this.GetBool("useWBack") || !target.IsDead)
            {
                return;
            }
            if (this.WActivated)
            {
                W.CastOnUnit(this.Player, this.Packets);
            }
            else if (this.RActivated)
            {
                R.CastOnUnit(this.Player, this.Packets);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }

            var useQ = this.GetBool("useQHarass");
            var useW = this.GetBool("useWHarass");
            var useWBack = this.GetBool("useWBackHarass");

            if (useQ)
            {
                Q.CastOnUnit(target, this.Packets);
            }

            if (useW && !this.WActivated && this.HasQBuff(target))
            {
                W.Cast(target, this.Packets);
            }

            if (useWBack && !this.HasQBuff(target) && this.WActivated)
            {
                W.Cast();
            }
        }

        /// <summary>
        ///     Called when the game is drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void Drawing_OnDraw(EventArgs args)
        {
            // Use position instead of server position for drawing
            var p = this.Player.Position;

            foreach (var spell in
                SpellList.Where(x => x.Slot != SpellSlot.R).Where(x => this.GetBool(string.Format("draw{0}", x.Slot))))
            {
                Render.Circle.DrawCircle(p, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        /// <summary>
        ///     Flees this instance.
        /// </summary>
        private void Flee()
        {
            var useJump = this.GetBool("Flee.UseW");
            var useE = this.GetBool("Flee.UseE");
            var doubleJump = this.GetBool("Flee.DoubleW");

            this.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            var closestEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget())
                    .OrderBy(x => x.Distance(this.Player))
                    .FirstOrDefault();

            var jumpedW = false;
            var jumpedR = false;

            if (useE && E.IsReady() && closestEnemy != null)
            {
                E.Cast(closestEnemy, this.Packets);
            }

            if (useJump && W.IsReady() && !this.WActivated)
            {
                W.Cast(Game.CursorPos, this.Packets);
                jumpedW = true;
            }
            else if (useJump && this.RStatus == RSpell.W && R.IsReady() && !this.RActivated)
            {
                R.Cast(Game.CursorPos, this.Packets);
                jumpedR = true;
            }

            if (doubleJump && (jumpedW || jumpedR))
            {
                if (jumpedW && R.IsReady() && this.RStatus == RSpell.W && !this.RActivated)
                {
                    R.Cast(Game.CursorPos, this.Packets);
                }

                else if (jumpedR && W.IsReady() && !this.WActivated)
                {
                    W.Cast(Game.CursorPos, this.Packets);
                }
            }
        }

        /// <summary>
        ///     Fired when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameOnOnGameUpdate(EventArgs args)
        {
            if (this.Menu.Item("Flee").GetValue<KeyBind>().Active)
            {
                this.Flee();
            }

            //Setup prediction for R spell
            switch (this.RStatus)
            {
                case RSpell.Q:
                    R = new Spell(SpellSlot.R, Q.Range);
                    break;
                case RSpell.W:
                    R = new Spell(SpellSlot.R, W.Range);
                    R.SetSkillshot(W.Delay, W.Width, W.Speed, W.Collision, SkillshotType.SkillshotCircle);
                    break;
                case RSpell.E:
                    R = new Spell(SpellSlot.R, E.Range);
                    R.SetSkillshot(E.Delay, E.Width, E.Speed, E.Collision, SkillshotType.SkillshotLine);
                    break;
            }

            switch (this.OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    this.DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    this.DoCombo();
                    break;
            }

            //DoCloneLogic();
        }

        /// <summary>
        ///     Fired on an interruptable target.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private void Interrupter_OnPossibleToInterrupt(
            Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!this.GetBool("eInterrupt") || args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            if (E.IsReady())
            {
                E.Cast(sender, this.Packets);
            }
            else if (R.IsReady() && this.RStatus == RSpell.E)
            {
                R.Cast(sender, this.Packets);
            }
        }

        // Mirror logic
        /// <summary>
        ///     Fired when a unit issues an order.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectIssueOrderEventArgs" /> instance containing the event data.</param>
        private void ObjAiBaseOnOnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Order.ToString().Contains("Pet"))
            {
                return;
            }

            if (this.Menu.Item("CloneLogic").GetValue<StringList>().SelectedValue != "Mirror Player")
            {
                return;
            }

            if (!this.HasValidClone)
            {
                return;
            }

            GameObjectOrder convertedOrder;
            switch (args.Order)
            {
                case GameObjectOrder.MoveTo:
                    convertedOrder = GameObjectOrder.MovePet;
                    break;
                case GameObjectOrder.AutoAttack:
                    convertedOrder = GameObjectOrder.AutoAttackPet;
                    break;
                default:
                    convertedOrder = args.Order;
                    break;
            }

            Game.PrintChat("Order: {0} Converted: {1}", args.Order.ToString(), convertedOrder.ToString());

            var clone = this.Player.Pet as Obj_AI_Base;
            if (clone != null && this.HasValidClone)
            {
                clone.IssueOrder(convertedOrder, args.Target);
                clone.IssueOrder(convertedOrder, args.TargetPosition);
            }
        }

        #endregion
    }
}