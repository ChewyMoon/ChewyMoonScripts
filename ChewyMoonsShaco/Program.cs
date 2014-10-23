#region

using LeagueSharp.Common;

#endregion

namespace ChewyMoonsShaco
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += ChewyMoonShaco.OnGameLoad;
        }
    }
}