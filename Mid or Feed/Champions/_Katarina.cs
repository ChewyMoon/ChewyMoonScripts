using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Mid_or_Feed.Champions
{
    class _Katarina : Plugin
    {
        public Spell Q;
        public Spell W;
        public Spell E;
        public Spell R;
        private bool overrideRProtection = false;

        private bool IsChannelingR
        {
            get { return Player.HasBuff("katarinarsound", true); }
        }
        public _Katarina()
        {
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);

            Q.SetTargetted(0.4f, 1800);

            PrintChat("Katarina loaded!");

            Game.OnUpdate += GameOnOnUpdate;
            Obj_AI_Base.OnIssueOrder += ObjAiBaseOnOnIssueOrder;
            Obj_AI_Base.OnProcessSpellCast += ObjAiBaseOnOnProcessSpellCast;
            Spellbook.OnStopCast += SpellbookOnOnStopCast;
            Spellbook.OnCastSpell += SpellbookOnOnCastSpell;
         }

        private void SpellbookOnOnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            {
                return;
            }

            if (IsChannelingR)
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

            if (!IsChannelingR || !sender.IsChanneling)
            {
                Orbwalker.SetMovement(true);
                Orbwalker.SetAttack(true);
            }
        }

        private void ObjAiBaseOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.SData.Name == "KatarinaR")
            {
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
            }

            if (args.SData.Name == "KatarinaE")
            {
                //Utility.DelayAction.Add(0, () => Player.IssueOrder(GameObjectOrder.MoveTo, ObjectManager.Player.Position));
            }
            
            
        }

        private void ObjAiBaseOnOnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!sender.IsMe || !GetBool("PreventUltCanceling"))
            {
                return;
            }

            if (!IsChannelingR)
            {
                return;
            }

            if (!overrideRProtection)
            {
                args.Process = false;
            }

            overrideRProtection = false;
        }

        private void GameOnOnUpdate(EventArgs args)
        {
            Console.Clear();
            Console.WriteLine(string.Join(" ", ObjectManager.Player.Buffs.Select(x => x.Name)));

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    DoLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }
        }

        private void DoCombo()
        {
            var mode = GetValue<StringList>("ComboMode").SelectedIndex;
            var target = TargetSelector.GetTarget(mode == 0 ? Q.Range : E.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = GetBool("UseQCombo");
            var useW = GetBool("UseWCombo");
            var useE = GetBool("UseECombo");
            var useR = GetBool("UseRCombo");
            var waitForQ = GetBool("WaitForQ");

            //QEW
            if (mode == 0)
            {
                if (Q.IsReady() && useQ && !IsChannelingR)
                {
                    Q.CastOnUnit(target);
                }

                if (E.IsReady() && useE && !IsChannelingR)
                {
                    if (waitForQ)
                    {
                        if (target.HasBuff("katarinaqmark", true))
                        {
                            E.CastOnUnit(target);
                        }
                    }
                    else
                    {
                        E.CastOnUnit(target);
                    }
                }

                if (W.IsReady() && useW && W.IsInRange(target) && !IsChannelingR)
                {
                    W.Cast();
                }
                else if (R.IsReady() && useR && R.IsInRange(target) && !IsChannelingR)
                {
                    Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player);

                    Orbwalker.SetMovement(false);
                    Orbwalker.SetAttack(false);

                    Utility.DelayAction.Add(Game.Ping / 2, () =>
                    {
                        R.Cast();
                    });            
                }
            }
            // EQW
            else if (mode == 1)
            {
                if (E.IsReady() && useE && !IsChannelingR)
                {
                    if (waitForQ)
                    {
                        if (target.HasBuff("katarinaqmark", true))
                        {
                            E.CastOnUnit(target);
                        }
                    }
                    else
                    {
                        E.CastOnUnit(target);
                    }
                }

                if (Q.IsReady() && useQ && !IsChannelingR)
                {
                    Q.CastOnUnit(target);
                }

                // Use else since w likes to cancel R
                if (W.IsReady() && useW && W.IsInRange(target) && !IsChannelingR)
                {
                    W.Cast();
                }
                else if (R.IsReady() && useR && R.IsInRange(target) && !IsChannelingR)
                {
                    Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player);

                    Orbwalker.SetMovement(false);
                    Orbwalker.SetAttack(false);

                    Utility.DelayAction.Add(
                        Game.Ping / 2, () =>
                        {
                            R.Cast();     
                        });
                }
            }

            if (target.IsDead)
            {
                // Cancel ult, allowing another combo
                overrideRProtection = true;
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
        }

        private void DoLaneClear()
        {
            throw new NotImplementedException();
        }

        private void DoHarass()
        {
            throw new NotImplementedException();
        }

        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            config.AddItem(new MenuItem("ComboMode", "Combo Mode:").SetValue(new StringList(new[] { "QEW", "EQW" })));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
        }

        public override void WaveClear(Menu config)
        {
            config.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWWaveClear", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEWaveClear", "Use E").SetValue(true));
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

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            config.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            config.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
        }
    }
}
