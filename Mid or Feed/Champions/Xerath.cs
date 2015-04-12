using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mid_or_Feed.Champions
{
    internal class Xerath : Plugin
    {
        public Xerath()
        {
            Q = new Spell(SpellSlot.Q, 800);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 1050);
            R = new Spell(SpellSlot.R, 3200);

            Q.SetSkillshot(0.6f, 100, int.MaxValue, false, SkillshotType.SkillshotLine);
            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 800, 1600, 1.45f);
            W.SetSkillshot(0.7f, 200, int.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.26f, 60, 1400, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 120, int.MaxValue, false, SkillshotType.SkillshotCircle);

            // Add items to spell list
            SpellList = new List<Spell> {E, Q, W, R};

            PrintChat("Xerath loaded!");

            Game.OnUpdate += GameOnOnUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        private Spell Q { get; set; }
        private Spell W { get; set; }
        private Spell E { get; set; }
        private Spell R { get; set; }
        private List<Spell> SpellList { get; set; }

        private bool IsChannelingR
        {
            get { return Player.HasBuff("xerathrshots", true); }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsValidTarget(E.Range) || !E.IsReady() || args.DangerLevel != Interrupter2.DangerLevel.High ||
                !GetBool("UseEInterrupt"))
            {
                return;
            }

            E.Cast(sender);
        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var unit = gapcloser.Sender;
            if (!unit.IsValidTarget())
            {
                return;
            }

            if (E.IsReady() && E.IsInRange(unit) && GetBool("UseEGapcloser"))
            {
                E.Cast(unit);
            }
            else if (W.IsReady() && W.IsInRange(unit) && GetBool("UseWGapcloser"))
            {
                W.Cast(unit);
            }
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("DrawQ");
            var drawW = GetBool("DrawW");
            var drawE = GetBool("DrawE");
            var drawR = GetBool("DrawR");
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

            if (drawR)
            {
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        private void GameOnOnUpdate(EventArgs args)
        {
            Console.Clear();
            Console.WriteLine(string.Join(" ", Player.Buffs.Select(x => x.Name)));

            if (GetBool("UseRKS"))
            {
                KillSecure();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LaneClear:
                    DoLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }
        }

        private void KillSecure()
        {
            var bestTarget =
                HeroManager.Enemies.Where(
                    x => x.IsValidTarget(R.Range) && Player.GetSpellDamage(x, SpellSlot.R)*3 > x.Health)
                    .OrderBy(x => x.Distance(Player))
                    .FirstOrDefault();

            if (!bestTarget.IsValidTarget())
            {
                return;
            }

            R.Cast(bestTarget);
        }

        private void DoLaneClear()
        {
            foreach (
                var spell in
                    SpellList.Where(x => x.IsReady() && x.Slot != SpellSlot.R && GetBool("Use" + x.Slot + "WaveClear")))
            {
                switch (spell.Slot)
                {
                    case SpellSlot.Q:
                    {
                        var farmLocation = Q.GetLineFarmLocation(MinionManager.GetMinions(1600));

                        if (farmLocation.MinionsHit <= 1)
                        {
                            continue;
                        }

                        if (!Q.IsCharging)
                        {
                            Q.StartCharging();
                        }
                        else
                        {
                            Q.Cast(farmLocation.Position);
                        }
                    }
                        break;
                    case SpellSlot.W:
                    {
                        var farmLocation = W.GetCircularFarmLocation(MinionManager.GetMinions(W.Range));

                        if (farmLocation.MinionsHit > 1)
                        {
                            W.Cast(farmLocation.Position);
                        }
                    }
                        break;
                    case SpellSlot.E:
                    {
                        var farmLocation = E.GetLineFarmLocation(MinionManager.GetMinions(E.Range));

                        if (farmLocation.MinionsHit > 1)
                        {
                            E.Cast(farmLocation.Position);
                        }
                    }
                        break;
                }
            }
        }

        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(1600, TargetSelector.DamageType.Magical);

            foreach (
                var spell in
                    SpellList.Where(x => x.IsReady() && x.Slot != SpellSlot.R && GetBool("Use" + x.Slot + "Combo")))
            {
                if (spell.Slot == SpellSlot.Q)
                {
                    if (!Q.IsCharging)
                    {
                        Q.StartCharging();
                    }
                    else
                    {
                        Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    }
                }
                else
                {
                    spell.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                }
            }

            if (!R.IsReady() || !GetBool("UseRCombo"))
            {
                return;
            }

            foreach (
                var enemy in
                    HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range))
                        .Where(
                            enemy =>
                                (!SpellList.Where(x => x.IsReady()).Any(x => x.IsInRange(enemy) && x.IsKillable(enemy))) ||
                                IsChannelingR))
            {
                R.Cast(enemy);
            }
        }

        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(1600, TargetSelector.DamageType.Magical);

            foreach (
                var spell in
                    SpellList.Where(x => x.IsReady() && x.Slot != SpellSlot.R && GetBool("Use" + x.Slot + "Harass")))
            {
                if (spell.Slot == SpellSlot.Q)
                {
                    if (!Q.IsCharging)
                    {
                        Q.StartCharging();
                    }
                    else
                    {
                        Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    }
                }
                else
                {
                    spell.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                }
            }
        }

        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRHarass", "Use R").SetValue(true));
        }

        public override void WaveClear(Menu config)
        {
            config.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWWaveClear", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEWaveClear", "Use E").SetValue(true));
        }

        public override void Misc(Menu config)
        {
            // Suggestions are nice!
            config.AddItem(new MenuItem("UseRKS", "KS with R").SetValue(true));
            config.AddItem(new MenuItem("UseEGapcloser", "E Gapcloser").SetValue(true));
            config.AddItem(new MenuItem("UseWGapcloser", "W Gapcloser").SetValue(true));
            config.AddItem(new MenuItem("UseEInterrupt", "Interrupt Spells with E").SetValue(true));
        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            config.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            config.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            config.AddItem(new MenuItem("DrawRMinimap", "Draw R on Minimap").SetValue(true));
        }
    }
}