using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace MoonDraven
{
    class Program
    {

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName == "Draven")
            {
                new MoonDraven().Load();
            }
        }
    }
}
