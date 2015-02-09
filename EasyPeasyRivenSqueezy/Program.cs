using LeagueSharp.Common;

namespace EasyPeasyRivenSqueezy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Riven.OnGameLoad;
        }
    }
}