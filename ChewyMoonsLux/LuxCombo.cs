using LeagueSharp;
using LeagueSharp.Common;
using System;

namespace ChewyMoonsLux
{
    class LuxCombo
    {
        private static bool _haveToAa;

        public static void OnGameUpdate(EventArgs args)
        {
            ChewyMoonsLux.PacketCast = ChewyMoonsLux.Menu.Item("packetCast").GetValue<bool>();

            if (ChewyMoonsLux.Menu.Item("Combo").GetValue<bool>())
            {
                Combo();
            }

        }

        internal static void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            _haveToAa = false;
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
            if (useQ && !_haveToAa)
            {
                // Add option to change hitchance? Idkkk
                ChewyMoonsLux.Q.CastIfHitchanceEquals(target, Prediction.HitChance.HighHitchance,
                    ChewyMoonsLux.PacketCast);
                if (aaAfterSpell)
                {
                    _haveToAa = true;
                    ChewyMoonsLux.Orbwalker.ForceTarget(target);
                }
            }

            if (useE && !_haveToAa)
            {
                ChewyMoonsLux.E.Cast(target, ChewyMoonsLux.PacketCast);
                if (aaAfterSpell)
                {
                    _haveToAa = true;
                    ChewyMoonsLux.Orbwalker.ForceTarget(target);
                }
            }

            if (useW)
            {
                ChewyMoonsLux.W.Cast(Game.CursorPos, ChewyMoonsLux.PacketCast);
            }

            if (!useR || _haveToAa) return;
            ChewyMoonsLux.R.Cast(target, ChewyMoonsLux.PacketCast);
            if (!aaAfterSpell) return;
            _haveToAa = true;
            ChewyMoonsLux.Orbwalker.ForceTarget(target);
        }
    }
}
