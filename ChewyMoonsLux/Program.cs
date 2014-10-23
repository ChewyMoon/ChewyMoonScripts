#region

using LeagueSharp.Common;

#endregion

namespace ChewyMoonsLux
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += ChewyMoonsLux.OnGameLoad;
        }
    }
}