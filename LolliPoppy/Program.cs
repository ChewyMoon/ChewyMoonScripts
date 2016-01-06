namespace LolliPoppy
{
    using LeagueSharp.Common;

    internal class Program
    {
        #region Methods

        /// <summary>
        /// The entry point of the application
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(System.EventArgs args)
        {
            
        }

        #endregion
    }
}