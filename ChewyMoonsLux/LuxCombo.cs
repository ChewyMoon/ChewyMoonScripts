using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChewyMoonsLux
{
    class LuxCombo
    {
        public static void OnGameUpdate(EventArgs args)
        {
            ChewyMoonsLux.PacketCast = ChewyMoonsLux.Menu.Item("packetCast").GetValue<bool>();

            if (ChewyMoonsLux.Menu.Item("Combo").GetValue<bool>())
            {
                Combo();
            }

        }

        private static void Combo()
        {
        }
    }
}
