using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace ChewyMoonsLux
{
    class QGapCloser
    {
        internal static void OnEnemyGapCloser(ActiveGapcloser gapcloser)
        {
            if (!ChewyMoonsLux.Menu.Item("antiGapCloserQ").GetValue<bool>()) return;
            ChewyMoonsLux.Q.Cast(gapcloser.Sender, ChewyMoonsLux.PacketCast);
        }
    }
}
