#region

using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Mid_or_Feed.Champions
{
    internal class Ahri : Plugin
    {
        //TODO: Implment some type of Ult logic.

        public Items.Item Dfg;
        public Spell E;
        public Spell Q;
        public Spell W;

        public Ahri()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 1000);

            Q.SetSkillshot(0.25f, 100, 2500, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 60, 1500, true, SkillshotType.SkillshotLine);

            Game.OnUpdate += GameOnOnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += InterrupterOnOnPossibleToInterrupt;
            Drawing.OnDraw += DrawingOnOnDraw;

            PrintChat("Ahri loaded.");
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("drawQ");
            var drawW = GetBool("drawW");
            var drawE = GetBool("drawE");
            var p = Player.Position;

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
        }

        private void InterrupterOnOnPossibleToInterrupt(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!GetBool("interruptE") || args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            E.Cast(sender, Packets);
        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!GetBool("gapcloseE"))
            {
                return;
            }

            E.Cast(gapcloser.Sender, Packets);
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
            switch (OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;

                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }
        }

        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            var useQ = GetBool("useQ");
            var useW = GetBool("useW");
            var useE = GetBool("useE");
            var useDfg = GetBool("useDFG");

            if (useDfg && Dfg.IsReady())
            {
                Dfg.Cast(target);
            }

            if (useE && E.IsReady())
            {
                E.Cast(target);
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target);
            }

            if (useW && W.IsReady() && W.IsInRange(target.ServerPosition, W.Range))
            {
                W.Cast(Packets);
            }
        }

        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (!GetBool("useQHarass") || !Q.IsReady())
            {
                return;
            }

            Q.Cast(target, Packets);
        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;

            if (Q.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1);
            }

            if (W.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
                dmg += dmg * 0.2;
            }

            if (!Dfg.IsReady())
            {
                return (float) dmg;
            }
            dmg += Player.GetItemDamage(target, Damage.DamageItems.Dfg);
            dmg += dmg * 0.2;

            return (float) dmg;
        }

        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            config.AddItem(new MenuItem("useE", "Use E").SetValue(true));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
        }

        public override void ItemMenu(Menu config)
        {
            config.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
        }

        public override void Misc(Menu config)
        {
            config.AddItem(new MenuItem("gapcloseE", "E on Gapcloser", true).SetValue(true));
            config.AddItem(new MenuItem("interruptE", "E to Interrupt").SetValue(true));
        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            config.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
        }
    }
}