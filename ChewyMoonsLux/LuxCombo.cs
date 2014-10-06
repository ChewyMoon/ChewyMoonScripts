using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System;

namespace ChewyMoonsLux
{
    internal class LuxCombo
    {
        private static bool _haveToAa;

        public static void OnGameUpdate(EventArgs args)
        {
            ChewyMoonsLux.PacketCast = ChewyMoonsLux.Menu.Item("packetCast").GetValue<bool>();

            if (ChewyMoonsLux.Menu.Item("ultKS").GetValue<bool>())
            {
                KillSecure();
            }

            if (ChewyMoonsLux.Menu.Item("combo").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (ChewyMoonsLux.Menu.Item("harass").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            if (ChewyMoonsLux.Menu.Item("autoShield").GetValue<KeyBind>().Active)
            {
                AutoShield();
            }

        }

        internal static void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe) return;
            _haveToAa = false;
        }

        private static void AutoShield()
        {
            // linq op babbyyy
            foreach (var teamMate in from teamMate in ObjectManager.Get<Obj_AI_Base>().Where(teamMate => teamMate.IsAlly && teamMate.IsValid) let hasToBePercent = ChewyMoonsLux.Menu.Item("autoShieldPercent").GetValue<int>() let ourPercent = teamMate.Health/teamMate.MaxHealth*100 where ourPercent <= hasToBePercent && ChewyMoonsLux.W.IsReady() select teamMate)
            {
                ChewyMoonsLux.W.Cast(teamMate);
            }
        }

        private static void KillSecure()
        {
            // KILL SECURE MY ASS LOOL
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget()).Where(hero => ObjectManager.Player.Distance(hero) <= ChewyMoonsLux.R.Range && ChewyMoonsLux.R.GetDamage(hero) >= hero.Health && ChewyMoonsLux.R.IsReady()))
            {
                ChewyMoonsLux.R.Cast(hero);
            }
        }

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

        private static void Combo()
        {
            var useQ = ChewyMoonsLux.Menu.Item("useQ").GetValue<bool>();
            var useW = ChewyMoonsLux.Menu.Item("useW").GetValue<bool>();
            var useE = ChewyMoonsLux.Menu.Item("useE").GetValue<bool>();
            var useR = ChewyMoonsLux.Menu.Item("useR").GetValue<bool>();

            var target = SimpleTs.GetTarget(ChewyMoonsLux.Q.Range, SimpleTs.DamageType.Magical);
            var aaAfterSpell = ChewyMoonsLux.Menu.Item("aaAfterSpell").GetValue<bool>();

            var useDfg = ChewyMoonsLux.Menu.Item("useDFG").GetValue<bool>();

            if (!target.IsValid || _haveToAa) return;

            if (useDfg)
            {
                if(Items.CanUseItem(3128) && Items.HasItem(3128)) Items.UseItem(3128, target);
            }

            if (ChewyMoonsLux.Q.IsReady() && useQ && !_haveToAa)
            {
                // Add option to change hitchance? Idkkk;
                var castedQ = ChewyMoonsLux.Q.CastIfHitchanceEquals(target, HitChance.High, ChewyMoonsLux.PacketCast);
                if (castedQ)
                {
                    if (aaAfterSpell)
                    {
                        _haveToAa = true;
                        ChewyMoonsLux.Orbwalker.ForceTarget(target);
                    }
                }
            }

            if (ChewyMoonsLux.E.IsReady() && useE && !_haveToAa)
            {
                ChewyMoonsLux.E.Cast(target, ChewyMoonsLux.PacketCast);
                if (aaAfterSpell)
                {
                    _haveToAa = true;
                    ChewyMoonsLux.Orbwalker.ForceTarget(target);
                }
            }

            if (ChewyMoonsLux.W.IsReady() && useW)
            {
                ChewyMoonsLux.W.Cast(Game.CursorPos, ChewyMoonsLux.PacketCast);
            }

            if (target.IsDead) return;
            if (!ChewyMoonsLux.R.IsReady() || !useR || _haveToAa) return;

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
