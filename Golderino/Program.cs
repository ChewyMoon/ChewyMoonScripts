using System;
using System.Resources;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace Golderino
{
    class Program
    {

        private static float _myTeamGold;
        private static float _enemyTeamGold;
        private static float _goldAdvantage;

        private static Render.Sprite greenBar;
        private static Render.Sprite redBar;

        public static int ImgWidth = 437;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            greenBar = new Render.Sprite(Resource1.bar_green, new Vector2(Drawing.Width / 2 - ImgWidth / 2, 100));
            redBar = new Render.Sprite(Resource1.bar_red, new Vector2(Drawing.Width / 2 - ImgWidth / 2, 100));
            
            redBar.Add();
            greenBar.Add();
            

            Game.OnGameUpdate += GameOnOnGameUpdate;

            Drawing.OnEndScene += delegate
            { redBar.OnEndScene(); greenBar.OnEndScene(); };

            Drawing.OnPostReset += delegate
            { redBar.OnPostReset(); greenBar.OnPostReset(); };

            Drawing.OnPreReset += delegate
            { redBar.OnPreReset(); greenBar.OnPreReset(); };

            Game.PrintChat("Golderino by ChewyMoon loaded.");
        }

        private static void ResetVariables()
        {
            _myTeamGold = 0;
            _enemyTeamGold = 0;
            _goldAdvantage = 0;
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            greenBar.Reset();
            ResetVariables();
            Console.Clear();
            
            foreach(var friend in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly))
            {
                _myTeamGold += friend.GoldEarned;
                Console.WriteLine(@"{0}: {1}", friend.ChampionName, friend.GoldEarned);
            }

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
            {
                _enemyTeamGold += enemy.GoldEarned;
                Console.WriteLine(@"{0}: {1}", enemy.ChampionName, enemy.GoldEarned);
            }

            var total = _myTeamGold + _enemyTeamGold;
            _goldAdvantage = (float) Math.Round(_myTeamGold/total*100, 1);

            var width = (_goldAdvantage/100)*ImgWidth;
            greenBar.Crop(new Rectangle(greenBar.X, greenBar.Y, (int) width, greenBar.Height), true);
            
        }

    }
}
