using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Brain
{
    internal class Brain
    {
        private static bool _killable;
        private static Obj_AI_Hero _target;

        private static double _myDamage;
        private static double _enemyDamage;

        private static int _myPercent;
        private static int _enemyPercent;

        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static void OnGameLoad(EventArgs args)
        {
            Config.CreateMenu();

            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;

            Util.PrintFancy("loaded. Created by ChewyMoon & TheFieryTaco");
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            try
            {
                var target = SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical);
                if (!target.IsValidTarget()) return;

                var ienumSpellslot = Enum.GetValues(typeof(SpellSlot)).Cast<SpellSlot>();
                var spellCombo = ienumSpellslot as SpellSlot[] ?? ienumSpellslot.ToArray();

                double myDamage = 0;
                double enemyDamage = 0;

                var myPercent = 0;
                var enemyPercent = 0;

                if (Config.CalcSpells) myDamage += Player.GetComboDamage(target, spellCombo);
                if (Config.CalcEnemySpells) enemyDamage += target.GetComboDamage(Player, spellCombo);

                _myDamage = myDamage;
                _enemyDamage = enemyDamage;

                _myPercent = myPercent;
                _enemyPercent = enemyPercent;

                _target = target;

                Console.WriteLine("Update {0}", myDamage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    public class Util
    {
        public static void PrintFancy(string msg)
        {
            Game.PrintChat("<font color=\"#6699ff\"><b>Brain:</b></font> <font color=\"#FFFFFF\">" + msg + "</font>");
        }
    }
}