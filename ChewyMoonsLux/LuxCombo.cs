using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System;

namespace ChewyMoonsLux
{
    internal class LuxCombo
    {
        private static readonly Dictionary<Obj_AI_Base, bool> AutoAttackDictionary = new Dictionary<Obj_AI_Base, bool>(); 

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

            if (ChewyMoonsLux.Menu.Item("harass").GetValue<KeyBind>().Active)
            {
               // Harass();
            }

            if (ChewyMoonsLux.Menu.Item("autoShield").GetValue<KeyBind>().Active)
            {
                AutoShield();
            }

        }

        private static void UpdateDictionary()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget()))
            {
                AutoAttackDictionary.Clear();
                AutoAttackDictionary.Add(enemy, enemy.HasBuff("luxilluminatingfraulein"));
            }

            // Debug information
            Console.Clear();
            foreach (var pair in AutoAttackDictionary)
            {
                Console.WriteLine("{0}: {1}", pair.Key.BaseSkinName, pair.Value);
            }
        }

        private static void AutoShield()
        {
            // linq op babbyyy
            foreach (var teamMate in from teamMate in ObjectManager.Get<Obj_AI_Base>().Where(teamMate => teamMate.IsAlly && teamMate.IsValid) let hasToBePercent = ChewyMoonsLux.Menu.Item("autoShieldPercent").GetValue<int>() let ourPercent = teamMate.Health/teamMate.MaxHealth*100 where ourPercent <= hasToBePercent && ChewyMoonsLux.W.IsReady() select teamMate)
            {
                ChewyMoonsLux.W.Cast(teamMate, ChewyMoonsLux.PacketCast);
            }
        }

        private static void KillSecure()
        {
            // KILL SECURE MY ASS LOOL
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget()).Where(hero => ObjectManager.Player.Distance(hero) <= ChewyMoonsLux.R.Range && ChewyMoonsLux.R.GetDamage(hero) >= hero.Health && ChewyMoonsLux.R.IsReady()))
            {
                ChewyMoonsLux.R.Cast(hero, ChewyMoonsLux.PacketCast);
            }
        }

        /*
        private static void Harass()
        {
            var useQ = ChewyMoonsLux.Menu.Item("useQHarass").GetValue<bool>();
            var useE = ChewyMoonsLux.Menu.Item("useEHarass").GetValue<bool>();
            var aaAfterSpell = ChewyMoonsLux.Menu.Item("aaHarass").GetValue<bool>();

            var target = SimpleTs.GetTarget(ChewyMoonsLux.Q.Range, SimpleTs.DamageType.Magical);
            if (!target.IsValid || _haveToAa) return;

            if (ChewyMoonsLux.Q.IsReady() && useQ && !_haveToAa)
            {
                ChewyMoonsLux.Q.CastIfHitchanceEquals(target, HitChance.High, ChewyMoonsLux.PacketCast);
                if (aaAfterSpell)
                {
                    _haveToAa = true;
                    ChewyMoonsLux.Orbwalker.ForceTarget(target);
                }
            }

            if (!ChewyMoonsLux.E.IsReady() || !useE || _haveToAa) return;
            ChewyMoonsLux.E.Cast(target, ChewyMoonsLux.PacketCast);

            if (!aaAfterSpell) return;
            _haveToAa = true;
            ChewyMoonsLux.Orbwalker.ForceTarget(target);
        }
        */
        private static void Combo()
        {
            var useQ = ChewyMoonsLux.Menu.Item("useQ").GetValue<bool>();
            var useW = ChewyMoonsLux.Menu.Item("useW").GetValue<bool>();
            var useE = ChewyMoonsLux.Menu.Item("useE").GetValue<bool>();
            var useR = ChewyMoonsLux.Menu.Item("useR").GetValue<bool>();

            var target = SimpleTs.GetTarget(ChewyMoonsLux.Q.Range, SimpleTs.DamageType.Magical);
            var aaAfterSpell = ChewyMoonsLux.Menu.Item("aaAfterSpell").GetValue<bool>();

            var useDfg = ChewyMoonsLux.Menu.Item("useDFG").GetValue<bool>();

            if (AutoAttackDictionary.Any(pair => pair.Key.Equals(target) && pair.Value && !aaAfterSpell))
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                return;
            }
            
            if (!target.IsValid) return;

            if (useDfg)
            {
                if(Items.CanUseItem(3128) && Items.HasItem(3128)) Items.UseItem(3128, target);
            }

            if (ChewyMoonsLux.Q.IsReady() && useQ)
            {
                var output = Prediction.GetPrediction(target, ChewyMoonsLux.Q.Delay, ChewyMoonsLux.Q.Range, ChewyMoonsLux.Q.Speed);
                if (output.AoeTargetsHitCount > 2) return;

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
                if (ChewyMoonsLux.R.GetDamage(target) >= target.Health)
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
