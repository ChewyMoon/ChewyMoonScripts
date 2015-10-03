namespace MoonDraven
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Program
    {
        #region Methods

        private static void GameOnOnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName == "Draven")
            {
                new MoonDraven().Load();
            }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        #endregion
    }
}