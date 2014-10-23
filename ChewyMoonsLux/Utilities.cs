#region

using LeagueSharp;

#endregion

namespace ChewyMoonsLux
{
    internal class Utilities
    {
        public static void PrintChat(string msg)
        {
            Game.PrintChat("<font color=\"#6699ff\"><b>ChewyMoon's Lux:</b></font> <font color=\"#FFFFFF\">" + msg +
                           "</font>");
        }
    }
}