using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace ChewyMoonsLux
{
    class Utilities
    {
        public static void PrintChat(string msg)
        {
            Game.PrintChat("<font color=\"#6699ff\"><b>ChewyMoon's Lux:</b></font> <font color=\"#FFFFFF\">" + msg + "</font>");
        }
    }
}
