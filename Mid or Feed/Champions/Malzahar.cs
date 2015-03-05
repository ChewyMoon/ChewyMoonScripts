using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mid_or_Feed.Champions
{
    internal class Malzahar : Plugin
    {
        public readonly List<Spell> SpellList;

        public Malzahar()
        {
            // Create spells
            var q = new Spell(SpellSlot.Q, 900);
            var w = new Spell(SpellSlot.W, 800);
            var e = new Spell(SpellSlot.E, 650);
            var r = new Spell(SpellSlot.R, 700);

            q.SetSkillshot(0.5f, 100, float.MaxValue, false, SkillshotType.SkillshotLine);
            w.SetSkillshot(0.5f, 240, 20, false, SkillshotType.SkillshotCircle);

            SpellList = new List<Spell> { q, w, e, r };


            PrintChat("Malzahar loaded.");

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += InterrupterOnOnPossibleToInterrupt;
        }

        private bool ShouldUseR(Obj_AI_Base target)
        {
            return Player.GetSpellDamage(target, SpellSlot.R) > target.Health;
        }

        private void InterrupterOnOnPossibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!unit.IsValidTarget())
            {
                return;
            }

            if (args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            if (!GetBool("InterruptQ"))
            {
                return;
            }

            GetSpell(SpellList, SpellSlot.Q).Cast(unit);
        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget())
            {
                return;
            }

            if (!GetBool("GapcloserQ"))
            {
                return;
            }

            GetSpell(SpellList, SpellSlot.Q).Cast(gapcloser.Sender);
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            foreach (var spell in SpellList.Where(x => GetBool("Draw" + x.Slot.ToString())))
            {
                Render.Circle.DrawCircle(Player.Position, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
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
            var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);

            if (Player.IsCastingInterruptableSpell() || Player.IsChannelingImportantSpell())
            {
                return;
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            foreach (var spell in SpellList.Where(x => x.IsReady()).Where(x => GetBool("Use" + x.Slot.ToString())))
            {
                if (spell.Slot != SpellSlot.R)
                {
                    spell.Cast(target, Packets);
                }
                else if(spell.Slot == SpellSlot.R && ShouldUseR(target))
                {
                    spell.CastOnUnit(target, Packets);
                }
            }
        }

        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            foreach (var spell in
                SpellList.Where(x => x.IsReady())
                    .Where(x => x.Slot != SpellSlot.R)
                    .Where(x => GetBool("Use" + x.Slot.ToString() + "Harass")))
            {
                spell.Cast(target, Packets);
            }
        }

        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseR", "Use R").SetValue(true));
        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ", "DrawQ").SetValue(true));
            config.AddItem(new MenuItem("DrawW", "DrawW").SetValue(true));
            config.AddItem(new MenuItem("DrawE", "DrawE").SetValue(true));
            config.AddItem(new MenuItem("DrawR", "DrawR").SetValue(true));
        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            return SpellList.Where(x => x.IsReady()).Sum(spell => spell.GetDamage(target));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
        }

        public override void Misc(Menu config)
        {
            config.AddItem(new MenuItem("InterruptQ", "Use Q to Interrupt").SetValue(true));
            config.AddItem(new MenuItem("GapcloserQ", "Use Q on Gapcloser").SetValue(true));
        }
    }
}