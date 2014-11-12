#region

using System;
using LeagueSharp.Common;

#endregion

namespace Sophies_Soraka
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("AY LMAO");
            CustomEvents.Game.OnGameLoad += SophiesSoraka.OnGameLoad;
        }
    }
}