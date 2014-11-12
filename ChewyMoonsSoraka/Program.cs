using LeagueSharp.Common;

namespace Sophies_Soraka
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += SophiesSoraka.OnGameLoad;
        }
    }
}
