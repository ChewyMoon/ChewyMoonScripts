#region

using System;
using System.Globalization;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace Golderino
{
    internal class Program
    {
        private static float _myTeamGold = 475 * 5;
        private static float _enemyTeamGold = 475 * 5;
        private static float _goldAdvantage;
        private static Render.Sprite _greenBar;
        private static Render.Sprite _redBar;
        private static Render.Text _leftText;
        private static Render.Text _middleText;
        private static Render.Text _rightText;
        public static int ImgWidth = 437;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            _greenBar = new Render.Sprite(Resource1.bar_green, new Vector2(Drawing.Width / 2 - ImgWidth / 2, 100));
            _redBar = new Render.Sprite(Resource1.bar_red, new Vector2(Drawing.Width / 2 - ImgWidth / 2, 100));

            _leftText = new Render.Text(
                "2375", _redBar.X - Drawing.GetTextExtent("2375").Width, _redBar.Y, 12, Color.White);
            _middleText = new Render.Text(
                "50%", Drawing.Width / 2 - Drawing.GetTextExtent("50%").Width / 2, _redBar.Y, 12, Color.White);
            _rightText = new Render.Text(
                "2375", _redBar.X + Drawing.GetTextExtent("2375").Width, _redBar.Y, 12, Color.White);

            _redBar.Add();
            _greenBar.Add();
            _leftText.Add();
            _middleText.Add();
            _rightText.Add();

            UpdateDrawings();

            Game.OnProcessPacket += Game_OnGameProcessPacket;
            Game.PrintChat("Golderino by ChewyMoon loaded.");
        }

        private static void UpdateDrawings()
        {
            _greenBar.Reset();

            _goldAdvantage = (float) Math.Round(_myTeamGold / (_myTeamGold + _enemyTeamGold) * 100, 1);

            var width = (_goldAdvantage / 100) * ImgWidth;
            _greenBar.Crop(new Rectangle(_greenBar.X, _greenBar.Y, (int) width, _greenBar.Height), true);

            _leftText.text = _myTeamGold.ToString(CultureInfo.InvariantCulture) + "g";
            _leftText.X = _redBar.X - Drawing.GetTextExtent(_leftText.text).Width;

            _middleText.text = _goldAdvantage + "%";
            _middleText.X = Drawing.Width / 2 -
                            Drawing.GetTextExtent(_goldAdvantage.ToString(CultureInfo.InvariantCulture)).Width / 2;

            _rightText.text = _enemyTeamGold.ToString(CultureInfo.InvariantCulture) + "g";
            _rightText.X = _redBar.Width + Drawing.GetTextExtent(_rightText.text).Width;

            Utility.DelayAction.Add(1000, UpdateDrawings);
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            var data = args.PacketData;
            if (data[0] != Packet.S2C.AddGold.Header)
            {
                return;
            }

            var decoded = Packet.S2C.AddGold.Decoded(data);
            Console.WriteLine(
                @"ReceivingUnit: {0} | SourceUnit {1}", decoded.ReceivingUnit.BaseSkinName,
                decoded.SourceUnit.BaseSkinName);
            if (decoded.ReceivingUnit.IsEnemy)
            {
                _enemyTeamGold += decoded.Gold;
            }
            else if (decoded.ReceivingUnit.IsAlly)
            {
                _myTeamGold += decoded.Gold;
            }
            else
            {
                Console.WriteLine(@"Uhh, Chewy? I think you dun goofed.");
            }
        }
    }
}