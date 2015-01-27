using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace Mid_or_Feed.Champions
{
    internal class Yasuo : Plugin
    {
        private static Vector3 _positionBeforeQE;
        public Items.Item Botrk;
        public Spell E;
        public Spell R;
        public Spell W;

        public Yasuo()
        {
            // Setup Spells
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 1200);

            E.SetTargetted(0.25f, 20);

            // Setup Items
            Botrk = ItemData.Blade_of_the_Ruined_King.GetItem();

            PrintChat("Yasuo loaded!");

            Game.OnGameUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += InterrupterOnOnPossibleToInterrupt;
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        public int QCount
        {
            get
            {
                if (Player.HasBuff("yasuoq", true))
                {
                    return 2;
                }

                return Player.HasBuff("yasuoQ3W", true) ? 3 : 1;
            }
        }

        public Spell Q
        {
            get
            {
                var spell = new Spell(SpellSlot.Q);

                switch (QCount)
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

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("DrawQ");
            var drawE = GetBool("DrawE");
            var drawR = GetBool("DrawR");
            var p = Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
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

        private void InterrupterOnOnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (QCount != 3 || !Q.IsReady() || !GetBool("UseQInterrupt") || !unit.IsValidTarget() ||
                spell.DangerLevel != InterruptableDangerLevel.High)
            {
                return;
            }

            Q.Cast(unit, Packets);
        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (QCount != 3 || !Q.IsReady() || !GetBool("UseQGapclose") || !gapcloser.Sender.IsValidTarget())
            {
                return;
            }

            Q.Cast(gapcloser.Sender, Packets);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            CheckAndUseR();

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
            var useQ = GetBool("UseQ");
            var useE = GetBool("UseE");
            var useEGapclose = GetBool("UseEGapclose");

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (target == null && useEGapclose && E.IsReady())
            {
                var targetExt = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);

                if (!targetExt.IsValidTarget())
                {
                    return;
                }

                var bestMinion =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(x => x.IsValidTarget(E.Range))
                        .Where(x => x.Distance(targetExt) < Player.Distance(targetExt))
                        .OrderByDescending(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestMinion != null)
                {
                    E.Cast(bestMinion, Packets);
                }

                return;
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            if (useE && useQ && Q.IsReady() && E.IsReady() && E.IsInRange(target, E.Range))
            {
                E.Cast(target, Packets);
                var castTime = (int) ((Player.Distance(target)/E.Speed) + E.Delay + Game.Ping/2f);

                Utility.DelayAction.Add(castTime, delegate { Q.Cast(target, Packets); });

                return;
            }

            if (Botrk.IsReady())
            {
                Botrk.Cast(target);
            }

            if (E.IsReady() && useE)
            {
                E.Cast(target, Packets);
            }

            if (Q.IsReady() && useQ)
            {
                Q.Cast(target, Packets);
            }
        }

        private void DoHarass()
        {
            var useQ = GetBool("UseQHarass");
            var useQE = GetBool("UseQEHarass");

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget())
            {
                return;
            }

            // Oh godd
            if (useQE && Q.IsReady() && E.IsReady() && E.IsInRange(target, E.Range))
            {
                _positionBeforeQE = Player.ServerPosition;

                E.Cast(target);

                var castTime = (int) ((Player.Distance(target)/E.Speed) + E.Delay + Game.Ping/2f);

                Utility.DelayAction.Add(castTime, delegate
                {
                    Q.Cast(target, Packets);

                    Utility.DelayAction.Add((int) (Q.Delay + Game.Ping/2f), delegate
                    {
                        // Find closest minion to last pos
                        var bestMinion =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(x => x.IsValidTarget(E.Range))
                                .OrderBy(x => x.Distance(_positionBeforeQE))
                                .FirstOrDefault();

                        if (bestMinion != null)
                        {
                            E.Cast(bestMinion, Packets);
                        }
                    });
                });
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target, Packets);
            }
        }

        private void CheckAndUseR()
        {
            if (!R.IsReady())
            {
                return;
            }

            var useR = GetBool("UseR");
            var autoREnemies = GetValue<Slider>("AutoUseR").Value;
            var useRDown = GetBool("UltDown");

            if (!useR)
            {
                return;
            }

            var enemiesKnockedUp =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget(R.Range))
                    .Where(x => x.HasBuffOfType(BuffType.Knockup));

            // Prevent Multiple Enumerations
            var enemies = enemiesKnockedUp as IList<Obj_AI_Hero> ?? enemiesKnockedUp.ToList();
            var killableEnemy = enemies.FirstOrDefault(x => R.GetDamage(x) > x.Health);
            if (killableEnemy != null)
            {
                if (!Q.IsReady())
                {
                    R.Cast(Packets);
                    return;
                }
            }

            if (autoREnemies > enemies.Count())
            {
                return;
            }

            if (useRDown)
            {
                // Get the lowest end time
                var lowestEndTime =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(R.Range))
                        .Where(x => x.HasBuffOfType(BuffType.Knockup))
                        .OrderBy(x => x.Buffs.First(buff => buff.Type == BuffType.Knockup).EndTime)
                        .Select(x => x.Buffs.First(buff => buff.Type == BuffType.Knockup).EndTime)
                        .First();

                var castTime = R.Delay + Game.Ping/2f;

                Utility.DelayAction.Add((int) ((lowestEndTime - Environment.TickCount) - castTime),
                    () => R.Cast(Packets));
            }
            else
            {
                R.Cast(Packets);
            }
        }

        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseEGapclose", "Use E Gapclose").SetValue(true));
            config.AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            config.AddItem(new MenuItem("AutoUseR", "Auto Use R on Enemies").SetValue(new Slider(3, 1, 5)));
        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            config.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            float dmg = 0;

            if (Q.IsReady())
            {
                dmg += Q.GetDamage(target);
            }

            if (E.IsReady())
            {
                dmg += E.GetDamage(target);
            }

            if (R.IsReady() && target.HasBuffOfType(BuffType.Knockup))
            {
                dmg += R.GetDamage(target);
            }

            if (Botrk.IsReady())
            {
                dmg += (float) Player.GetItemDamage(target, Damage.DamageItems.Botrk);
            }

            if (Orbwalking.CanAttack())
            {
                // Include the BotRK passive
                dmg += (float) Player.GetAutoAttackDamage(target, true);
            }

            return dmg;
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            // config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseQEHarass", "Use QE").SetValue(true));
            config.AddItem(new MenuItem("info1", "If QE is on, It will QE enemy"));
            config.AddItem(new MenuItem("info2", "And E back to location you were"));
        }

        public override void ItemMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseBotRK", "Use BotRK").SetValue(true));
        }

        public override void Misc(Menu config)
        {
            config.AddItem(new MenuItem("UseQInterrupt", "Use Q3 to Interrupt").SetValue(true));
            config.AddItem(new MenuItem("UseQGapclose", "Use Q3 Gapcloser").SetValue(true));
            config.AddItem(new MenuItem("UltDown", "Use R When Falling Down").SetValue(true));
        }
    }
}