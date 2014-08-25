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

        }

        internal static void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            _haveToAa = false;
        }

        private static void KillSecure()
        {
            // KILL SECURE MY ASS LOOL
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsValidTarget()) return;
                if (SharpDX.Vector2.Distance(hero.ServerPosition.To2D(), hero.ServerPosition.To2D()) > ChewyMoonsLux.R.Range) return;
                if (ChewyMoonsLux.R.GetDamage(hero) < hero.Health) return;

                ChewyMoonsLux.R.Cast(hero, ChewyMoonsLux.PacketCast);
                return;
            }
        }

        private static void Harass()
        {
            var useQ = ChewyMoonsLux.Menu.Item("useQHarass").GetValue<bool>();
            var useE = ChewyMoonsLux.Menu.Item("useEHarass").GetValue<bool>();
            var aaAfterSpell = ChewyMoonsLux.Menu.Item("aaHarass").GetValue<bool>();

            var target = SimpleTs.GetTarget(ChewyMoonsLux.Q.Range, SimpleTs.DamageType.Magical);
            if (!target.IsValid || !_haveToAa) return;

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

            if (!target.IsValid || _haveToAa) return;
            if (ChewyMoonsLux.Q.IsReady() && useQ && !_haveToAa)
            {
                // Add option to change hitchance? Idkkk
                ChewyMoonsLux.Q.CastIfHitchanceEquals(target, HitChance.High, ChewyMoonsLux.PacketCast);
                if (aaAfterSpell)
                {
                    _haveToAa = true;
                    ChewyMoonsLux.Orbwalker.ForceTarget(target);
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

            if (!ChewyMoonsLux.R.IsReady() || !useR || _haveToAa) return;
            ChewyMoonsLux.R.Cast(target, ChewyMoonsLux.PacketCast);
            if (!aaAfterSpell) return;
            _haveToAa = true;
            ChewyMoonsLux.Orbwalker.ForceTarget(target);
        }
    }
}
