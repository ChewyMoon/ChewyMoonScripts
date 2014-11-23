using System;
using System.Resources;
using System.Collections.Generic;
using System.Globalization;
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

        private static Render.Sprite _greenBar;
        private static Render.Sprite _redBar;

        private static Render.Text leftText;
        private static Render.Text middleText;
        private static Render.Text rightText;


        public static int ImgWidth = 437;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            _greenBar = new Render.Sprite(Resource1.bar_green, new Vector2(Drawing.Width / 2 - ImgWidth / 2, 100));
            _redBar = new Render.Sprite(Resource1.bar_red, new Vector2(Drawing.Width / 2 - ImgWidth / 2, 100));

            leftText = new Render.Text("2375", _redBar.X - Drawing.GetTextExtent("2375").Width, _redBar.Y, 12, Color.White);
            middleText = new Render.Text("50%", Drawing.Width/2 - Drawing.GetTextExtent("50%").Width/2, _redBar.Y ,12, Color.White);
            rightText = new Render.Text("2375", _redBar.X + Drawing.GetTextExtent("2375").Width, _redBar.Y, 12, Color.White);

            _redBar.Add();
            _greenBar.Add();
            leftText.Add();
            middleText.Add();
            rightText.Add();

            Drawing.OnEndScene += delegate
            { _redBar.OnEndScene(); _greenBar.OnEndScene(); };

            Drawing.OnPostReset += delegate
            { _redBar.OnPostReset(); _greenBar.OnPostReset(); };

            Drawing.OnPreReset += delegate
            { _redBar.OnPreReset(); _greenBar.OnPreReset(); };

            UpdateGold();

            Game.PrintChat("Golderino by ChewyMoon loaded.");
        }

        private static void UpdateGold()
        {
            _greenBar.Reset();
            ResetVariables();
            Console.Clear();

            foreach (var friend in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly))
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
            _goldAdvantage = (float)Math.Round(_myTeamGold / total * 100, 1);

            var width = (_goldAdvantage / 100) * ImgWidth;
            _greenBar.Crop(new Rectangle(_greenBar.X, _greenBar.Y, (int)width, _greenBar.Height), true);

            leftText.text = _myTeamGold.ToString(CultureInfo.InvariantCulture) + "g";
            leftText.X = _redBar.X - Drawing.GetTextExtent(leftText.text).Width;

            middleText.text = _goldAdvantage + "%";
            middleText.X = Drawing.Width/2 - Drawing.GetTextExtent(_goldAdvantage.ToString(CultureInfo.InvariantCulture)).Width/2;

            rightText.text = _enemyTeamGold.ToString(CultureInfo.InvariantCulture) + "g";
            rightText.X = _redBar.X + Drawing.GetTextExtent(rightText.text).Width;

            Utility.DelayAction.Add(1000, UpdateGold);
        }

        private static void ResetVariables()
        {
            _myTeamGold = 0;
            _enemyTeamGold = 0;
            _goldAdvantage = 0;
        }
    }
}
