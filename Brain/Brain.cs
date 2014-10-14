using LeagueSharp;
using LeagueSharp.Common;
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
        private double enemyDamage;

        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static void OnGameLoad(EventArgs args)
        {
            Config.CreateMenu();

            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;

            Util.PrintFancy("Loaded. Created by ChewyMoon & TheFieryTaco");
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            //Utility.PrintFloatText(ObjectManager.Player, _myDamage.ToString(CultureInfo.InvariantCulture), Packet.FloatTextPacket.Special);
            Drawing.DrawText(Player.Position.X, Player.Position.Y, Color.Crimson, _myDamage.ToString(CultureInfo.InvariantCulture));
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            // Temp TS
            var target = SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical);
            var ienumSpellslot = Enum.GetValues(typeof(SpellSlot)).GetEnumerator() as IEnumerable<SpellSlot>;

            if (ienumSpellslot == null) return;
            var spellCombo = ienumSpellslot as SpellSlot[] ?? ienumSpellslot.ToArray();

            var myDamage = Player.GetComboDamage(target, spellCombo);
            var enemyDamage = target.GetComboDamage(Player, spellCombo);

            _myDamage = myDamage;
            _target = target;
        }
    }

    public class Util
    {
        public static void PrintFancy(string msg)
        {
            Game.PrintChat("<font color=\"#6699ff\"><b>Brain: </b></font> <font color=\"#FFFFFF\">" + msg + "</font>");
        }
    }
}