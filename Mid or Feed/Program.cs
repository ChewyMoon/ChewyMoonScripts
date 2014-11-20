#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Mid_or_Feed
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            var plugin = Type.GetType("Mid_or_Feed.Champions." + ObjectManager.Player.ChampionName);

            if (plugin == null)
            {
                Plugin.PrintChat(ObjectManager.Player.ChampionName + " not supported!");
                return;
            }

            Activator.CreateInstance(plugin);
        }

        private static void CurrentDomainOnUnhandledException(object sender,
            UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Console.WriteLine(((Exception) unhandledExceptionEventArgs.ExceptionObject).Message);
            Plugin.PrintChat("encountered an error! ChewyMoon dun goofed again gg");
        }
    }
}