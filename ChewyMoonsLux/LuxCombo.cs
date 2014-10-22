using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChewyMoonsLux
{
    internal class LuxCombo
    {
        private static readonly Dictionary<Obj_AI_Hero, bool> AutoAttackDictionary = new Dictionary<Obj_AI_Hero, bool>();

        public static void OnGameUpdate(EventArgs args)
        {
            ChewyMoonsLux.PacketCast = ChewyMoonsLux.Menu.Item("packetCast").GetValue<bool>();

            UpdateDictionary();

            if (ChewyMoonsLux.Menu.Item("ultKS").GetValue<bool>())
            {
                KillSecure();
            }

            // should be reversed but w/e
            if (Orbwalking.OrbwalkingMode.Combo == ChewyMoonsLux.Orbwalker.ActiveMode)
            {
                Combo();
            }

            if (ChewyMoonsLux.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }

            if (ChewyMoonsLux.Menu.Item("autoShield").GetValue<KeyBind>().Active)
            {
                AutoShield();
            }
        }

        private static void UpdateDictionary()
        {
            AutoAttackDictionary.Clear();

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget()))
            {
                AutoAttackDictionary.Add(enemy, enemy.HasBuff("luxilluminatingfraulein"));
            }
        }

        private static void AutoShield()
        {
            // linq op babbyyy
            foreach (var teamMate in from teamMate in ObjectManager.Get<Obj_AI_Base>().Where(teamMate => teamMate.IsAlly && teamMate.IsValid) let hasToBePercent = ChewyMoonsLux.Menu.Item("autoShieldPercent").GetValue<int>() let ourPercent = teamMate.Health / teamMate.MaxHealth * 100 where ourPercent <= hasToBePercent && ChewyMoonsLux.W.IsReady() select teamMate)
            {
                ChewyMoonsLux.W.Cast(teamMate, ChewyMoonsLux.PacketCast);
            }
        }

        private static void KillSecure()
        {
            // KILL SECURE MY ASS LOOL
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget()).Where(hero => ObjectManager.Player.Distance(hero) <= ChewyMoonsLux.R.Range && ObjectManager.Player.GetSpellDamage(hero, SpellSlot.Q) >= hero.Health && ChewyMoonsLux.R.IsReady()))
            {
                ChewyMoonsLux.R.Cast(hero, ChewyMoonsLux.PacketCast);
            }
        }

        private static void Harass()
        {
            var useQ = ChewyMoonsLux.Menu.Item("useQHarass").GetValue<bool>();
            var useE = ChewyMoonsLux.Menu.Item("useEHarass").GetValue<bool>();
            var aaAfterSpell = ChewyMoonsLux.Menu.Item("aaHarass").GetValue<bool>();

            var target = SimpleTs.GetTarget(ChewyMoonsLux.Q.Range, SimpleTs.DamageType.Magical);
            if (!target.IsValidTarget() || target == null) return;

            if (AutoAttackDictionary.Any(pair => pair.Key.BaseSkinName == target.BaseSkinName && pair.Value && aaAfterSpell))
            {
                ChewyMoonsLux.Orbwalker.ForceTarget(target);
                return;
            }

            if (useQ && ChewyMoonsLux.Q.IsReady())
            {
                var input = new PredictionInput()
                {
                    Unit = target,
                    Delay = ChewyMoonsLux.Q.Delay,
                    Range = ChewyMoonsLux.Q.Range,
                    Speed = ChewyMoonsLux.Q.Speed
                };

                var output = Prediction.GetPrediction(input);

                if (SpellCombo.AnalyzeQ(new PredictionInput(), output)) return;

                ChewyMoonsLux.Q.Cast(output.CastPosition, ChewyMoonsLux.PacketCast);

                if (aaAfterSpell)
                    return;
            }

            if (!useE || !ChewyMoonsLux.E.IsReady()) return;
            ChewyMoonsLux.E.Cast(target, ChewyMoonsLux.PacketCast);
            if (aaAfterSpell)
            {
                return;
            }
        }

        private static void Combo()
        {
            var useQ = ChewyMoonsLux.Menu.Item("useQ").GetValue<bool>();
            var useW = ChewyMoonsLux.Menu.Item("useW").GetValue<bool>();
            var useE = ChewyMoonsLux.Menu.Item("useE").GetValue<bool>();
            var useR = ChewyMoonsLux.Menu.Item("useR").GetValue<bool>();

            var target = SimpleTs.GetTarget(ChewyMoonsLux.Q.Range, SimpleTs.DamageType.Magical);
            var aaAfterSpell = ChewyMoonsLux.Menu.Item("aaAfterSpell").GetValue<bool>();

            var useDfg = ChewyMoonsLux.Menu.Item("useDFG").GetValue<bool>();

            if (AutoAttackDictionary.Any(pair => pair.Key.BaseSkinName == target.BaseSkinName && pair.Value && aaAfterSpell))
            {
                ChewyMoonsLux.Orbwalker.ForceTarget(target);
                return;
            }

            if (!target.IsValid) return;

            if (useDfg)
            {
                if (Items.CanUseItem(3128) && Items.HasItem(3128)) Items.UseItem(3128, target);
            }

            if (ChewyMoonsLux.Q.IsReady() && useQ)
            {
                var input = new PredictionInput()
                {
                    Unit = target,
                    Delay = ChewyMoonsLux.Q.Delay,
                    Range = ChewyMoonsLux.Q.Range,
                    Speed = ChewyMoonsLux.Q.Speed
                };

                var output = Prediction.GetPrediction(input);

                if (SpellCombo.AnalyzeQ(new PredictionInput(), output)) return;

                ChewyMoonsLux.Q.Cast(output.CastPosition, ChewyMoonsLux.PacketCast);

                if (aaAfterSpell)
                    return;
            }

            if (ChewyMoonsLux.E.IsReady() && useE)
            {
                ChewyMoonsLux.E.Cast(target, ChewyMoonsLux.PacketCast);
                if (aaAfterSpell)
                {
                    return;
                }
            }

            if (ChewyMoonsLux.W.IsReady() && useW)
            {
                ChewyMoonsLux.W.Cast(Game.CursorPos, ChewyMoonsLux.PacketCast);
            }

            if (target.IsDead) return;
            if (!ChewyMoonsLux.R.IsReady() || !useR) return;

            if (ChewyMoonsLux.Menu.Item("onlyRIfKill").GetValue<bool>())
            {
                if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) >= target.Health)
                {
                    ChewyMoonsLux.R.Cast(target, ChewyMoonsLux.PacketCast);
                }
            }
            else
            {
                ChewyMoonsLux.R.Cast(target, ChewyMoonsLux.PacketCast);
            }
        }
    }
}