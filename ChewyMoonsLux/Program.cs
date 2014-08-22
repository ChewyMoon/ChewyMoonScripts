using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoonsLux
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ObjectManager.Player.BaseSkinName != "Lux") return;

            Game.PrintChat("ChewyMoon's Lux is under development, sorry if SVNCompiler built it!");

            CustomEvents.Game.OnGameLoad += ChewyMoonsLux.OnGameLoad;
        }
    }
}
