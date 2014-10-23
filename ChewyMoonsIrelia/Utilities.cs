#region

using LeagueSharp;

#endregion

namespace ChewyMoonsIrelia
{
    public class Utilities
    {
        /// <summary>
        ///     Thanks honda <3
        /// </summary>
        /// <param name="msg"></param>
        public static void PrintChat(string msg)
        {
            Game.PrintChat("<font color=\"#6699ff\"><b>ChewyMoon's Irelia:</b></font> <font color=\"#FFFFFF\">" + msg +
                           "</font>");
        }
    }
}