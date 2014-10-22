using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChewyMoonsShaco
{
    internal class ChewyMoonShaco
    {
        public static Spell Q;

        public static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Shaco") return;

            Game.OnGameUpdate += GameOnOnGameUpdate;
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            Console.Clear();
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
            {
                Console.WriteLine("{0}: {1}", enemy.BaseSkinName, enemy.Orientation);
            }
        }
    }
}