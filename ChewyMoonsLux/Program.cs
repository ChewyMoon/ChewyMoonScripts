using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoonsLux
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ObjectManager.Player.ChampionName != "Lux") return;

            Game.PrintChat("ChewyMoon's Lux is under development, sorry if SVNCompiler built it!");

            CustomEvents.Game.OnGameLoad += ChewyMoonsLux.OnGameLoad;
        }
    }
}
