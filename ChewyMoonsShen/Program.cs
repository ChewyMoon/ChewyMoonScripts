using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoonsShen
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ObjectManager.Player.BaseSkinName != "Shen") return;
            CustomEvents.Game.OnGameEnd += ChewyMoonsShen.OnGameLoad;
        }
    }
}
