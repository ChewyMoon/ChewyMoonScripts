#region

using LeagueSharp.Common;

#endregion

namespace Sophies_Soraka
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += SophiesSoraka.OnGameLoad;
        }
    }
}