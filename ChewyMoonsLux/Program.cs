using LeagueSharp;
using LeagueSharp.Common;

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