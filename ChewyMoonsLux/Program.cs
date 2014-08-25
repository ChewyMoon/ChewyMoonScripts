using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoonsLux
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ObjectManager.Player.ChampionName != "Lux") return;

            CustomEvents.Game.OnGameLoad += ChewyMoonsLux.OnGameLoad;
        }
    }
}
