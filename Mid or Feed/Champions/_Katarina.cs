namespace Mid_or_Feed.Champions
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class _Katarina : Plugin
    {
        #region Fields

        public Spell E;

        public Spell Q;

        public Spell R;

        public Spell W;

        private bool overrideRProtection = false;

        #endregion

        #region Constructors and Destructors

        public _Katarina()
        {
            this.Q = new Spell(SpellSlot.Q, 675);
            this.W = new Spell(SpellSlot.W, 400);
            this.E = new Spell(SpellSlot.E, 700);
            this.R = new Spell(SpellSlot.R, 550);

            this.Q.SetTargetted(0.4f, 1800);

            PrintChat("Katarina loaded!");

            Game.OnUpdate += this.GameOnOnUpdate;
            Obj_AI_Base.OnIssueOrder += this.ObjAiBaseOnOnIssueOrder;
            Obj_AI_Base.OnProcessSpellCast += this.ObjAiBaseOnOnProcessSpellCast;
            Spellbook.OnStopCast += this.SpellbookOnOnStopCast;
            Spellbook.OnCastSpell += this.SpellbookOnOnCastSpell;
        }

        #endregion

        #region Properties

        private bool IsChannelingR
        {
            get
            {
                return this.Player.HasBuff("katarinarsound", true);
            }
        }

        #endregion

        #region Public Methods and Operators

        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            config.AddItem(new MenuItem("ComboMode", "Combo Mode:").SetValue(new StringList(new[] { "QEW", "EQW" })));
        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            config.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            config.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
        }

        public override void ItemMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseHextech", "Use Hextech Gunblade").SetValue(true));
            config.AddItem(new MenuItem("UseCutlass", "Use Bilgewater Cutlass").SetValue(true));
        }

        public override void Misc(Menu config)
        {
            config.AddItem(new MenuItem("WaitForQ", "Wait for Q Mark before E/W").SetValue(false));
            config.AddItem(
                new MenuItem("PreventUltCanceling", "Prevent Ult Canceling(Evade, Mouse clicking..)").SetValue(true));
            //TODO: KS stuff XD
        }

        public override void WaveClear(Menu config)
        {
            config.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWWaveClear", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEWaveClear", "Use E").SetValue(true));
        }

        #endregion

        #region Methods

        private void DoCombo()
        {
            var mode = this.GetValue<StringList>("ComboMode").SelectedIndex;
            var target = TargetSelector.GetTarget(
                mode == 0 ? this.Q.Range : this.E.Range,
                TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = this.GetBool("UseQCombo");
            var useW = this.GetBool("UseWCombo");
            var useE = this.GetBool("UseECombo");
            var useR = this.GetBool("UseRCombo");
            var waitForQ = this.GetBool("WaitForQ");

            //QEW
            if (mode == 0)
            {
                if (this.Q.IsReady() && useQ && !this.IsChannelingR)
                {
                    this.Q.CastOnUnit(target);
                }

                if (this.E.IsReady() && useE && !this.IsChannelingR)
                {
                    if (waitForQ)
                    {
                        if (target.HasBuff("katarinaqmark", true))
                        {
                            this.E.CastOnUnit(target);
                        }
                    }
                    else
                    {
                        this.E.CastOnUnit(target);
                    }
                }

                if (this.W.IsReady() && useW && this.W.IsInRange(target) && !this.IsChannelingR)
                {
                    this.W.Cast();
                }
                else if (this.R.IsReady() && useR && this.R.IsInRange(target) && !this.IsChannelingR)
                {
                    this.Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player);

                    this.Orbwalker.SetMovement(false);
                    this.Orbwalker.SetAttack(false);

                    Utility.DelayAction.Add(Game.Ping / 2, () => { this.R.Cast(); });
                }
            }
            // EQW
            else if (mode == 1)
            {
                if (this.E.IsReady() && useE && !this.IsChannelingR)
                {
                    if (waitForQ)
                    {
                        if (target.HasBuff("katarinaqmark", true))
                        {
                            this.E.CastOnUnit(target);
                        }
                    }
                    else
                    {
                        this.E.CastOnUnit(target);
                    }
                }

                if (this.Q.IsReady() && useQ && !this.IsChannelingR)
                {
                    this.Q.CastOnUnit(target);
                }

                // Use else since w likes to cancel R
                if (this.W.IsReady() && useW && this.W.IsInRange(target) && !this.IsChannelingR)
                {
                    this.W.Cast();
                }
                else if (this.R.IsReady() && useR && this.R.IsInRange(target) && !this.IsChannelingR)
                {
                    this.Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player);

                    this.Orbwalker.SetMovement(false);
                    this.Orbwalker.SetAttack(false);

                    Utility.DelayAction.Add(Game.Ping / 2, () => { this.R.Cast(); });
                }
            }

            if (target.IsDead)
            {
                // Cancel ult, allowing another combo
                this.overrideRProtection = true;
                this.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
        }

        private void DoHarass()
        {
            throw new NotImplementedException();
        }

        private void DoLaneClear()
        {
            throw new NotImplementedException();
        }

        private void GameOnOnUpdate(EventArgs args)
        {
            Console.Clear();
            Console.WriteLine(string.Join(" ", ObjectManager.Player.Buffs.Select(x => x.Name)));

            switch (this.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    this.DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    this.DoLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    this.DoCombo();
                    break;
            }
        }

        private void ObjAiBaseOnOnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!sender.IsMe || !this.GetBool("PreventUltCanceling"))
            {
                return;
            }

            if (!this.IsChannelingR)
            {
                return;
            }

            if (!this.overrideRProtection)
            {
                args.Process = false;
            }

            this.overrideRProtection = false;
        }

        private void ObjAiBaseOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.SData.Name == "KatarinaR")
            {
                this.Orbwalker.SetAttack(false);
                this.Orbwalker.SetMovement(false);
            }

            if (args.SData.Name == "KatarinaE")
            {
                //Utility.DelayAction.Add(0, () => Player.IssueOrder(GameObjectOrder.MoveTo, ObjectManager.Player.Position));
            }
        }

        private void SpellbookOnOnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            {
                return;
            }

            if (this.IsChannelingR)
            {
                args.Process = false;
            }
        }

        private void SpellbookOnOnStopCast(Spellbook sender, SpellbookStopCastEventArgs args)
        {
            if (!sender.Owner.IsMe)
            {
                return;
            }

            if (!this.IsChannelingR || !sender.IsChanneling)
            {
                this.Orbwalker.SetMovement(true);
                this.Orbwalker.SetAttack(true);
            }
        }

        #endregion
    }
}