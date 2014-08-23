using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoonsLux
{
    class LuxCombo
    {
        private static bool haveToAA = false;

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
            haveToAA = false;
        }

        private static void Combo()
        {
            var useQ = ChewyMoonsLux.Menu.Item("useQ").GetValue<bool>();
            var useW = ChewyMoonsLux.Menu.Item("useW").GetValue<bool>();
            var useE = ChewyMoonsLux.Menu.Item("useE").GetValue<bool>();
            var useR = ChewyMoonsLux.Menu.Item("useR").GetValue<bool>();

            var target = SimpleTs.GetTarget(ChewyMoonsLux.Q.Range, SimpleTs.DamageType.Magical);

            if (!target.IsValid || haveToAA) return;



        }
    }
}
