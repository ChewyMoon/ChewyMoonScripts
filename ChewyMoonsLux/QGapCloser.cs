#region

using LeagueSharp.Common;

#endregion

namespace ChewyMoonsLux
{
    internal class QGapCloser
    {
        internal static void OnEnemyGapCloser(ActiveGapcloser gapcloser)
        {
            if (!ChewyMoonsLux.Menu.Item("antiGapCloserQ").GetValue<bool>()) return;
            ChewyMoonsLux.Q.Cast(gapcloser.Sender, ChewyMoonsLux.PacketCast);
        }
    }
}