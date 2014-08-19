using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Win32;

namespace ChewyMoonsIrelia
{
    class Program
    {
        private const string ChampName = "Irelia";
        private const string Version = "0.1";

        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;

// ReSharper disable InconsistentNaming
        private static Spell Q, W, E, R;
// ReSharper restore InconsistentNaming

        // Irelia Ultimate stuff.
        private static bool _hasToFire = false;
        private static int _charges = 0;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampName)
                return;

            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, ObjectManager.Player.AttackRange); // So confused.
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);

            Q.SetSkillshot(0.25f, 75f, 1500f, false, Prediction.SkillshotType.SkillshotLine);
            E.SetSkillshot(0.15f, 75f, 1500f, false, Prediction.SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.15f, 80f, 1500f, false, Prediction.SkillshotType.SkillshotLine);

            SetupMenu();

            Utilities.PrintChat("Loaded version (" + Version + ")");
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {

            FireCharges();

            if (!Orbwalking.CanMove(100)) return;

            if (_menu.Item("comboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else if (_menu.Item("harassActive").GetValue<KeyBind>().Active)
            {
                Harass();
            }
        }

        private static void FireCharges()
        {
            if (!_hasToFire) return;

            R.Cast(SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical)); //Dunnno

            _charges -= 1;

            _hasToFire = _charges != 0;


        }

        private static void Combo()
        {
            // Simple combo q -> w -> e -> r

            var useQ = _menu.Item("useQ").GetValue<bool>();
            var useW = _menu.Item("useW").GetValue<bool>();
            var useE = _menu.Item("useE").GetValue<bool>();
            var useR = _menu.Item("useR").GetValue<bool>();
            var useEStun = _menu.Item("useEStun").GetValue<bool>();

            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            // start with w.
            if (useW && W.IsReady())
            {
                W.Cast();
            }

            // follow up with q
            if (useQ && Q.IsReady())
            {
                Q.Cast(target);
            }

            // stunerino
            if (useE && E.IsReady())
            {
                if (useEStun)
                {
                    if (CanStunTarget(target))
                    {
                        E.Cast(target);
                    }
                }
                else
                {
                    E.Cast(target);
                }
            }
            
            // Resharper did this, IDK if it works.
            // Original code:  if (useR && R.IsReady() && !hasToFire)
            if (!useR || !R.IsReady() || _hasToFire) return;
            _hasToFire = true;
            _charges = 4;
        }

        private static void Harass()
        {
            // not implemented yet :/
        }

        private static bool CanStunTarget(AttackableUnit target)
        {
            return ObjectManager.Player.Health < target.Health;
        }

        private static void SetupMenu()
        {
            _menu = new Menu("--[ChewyMoon's Irelia]--", "cmIrelia", true);

            // Target Selector
            var targetSelectorMenu = new Menu("[ChewyMoon's Irelia] - TS", "cmIreliaTS");
            SimpleTs.AddToMenu(targetSelectorMenu);
            _menu.AddSubMenu(targetSelectorMenu);

            // Orbwalker
            var orbwalkerMenu = new Menu("[ChewyMoon's Irelia] - Orbwalker", "cmIreliaOW");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            // Combo
            var comboMenu = new Menu("[ChewyMoon's Irelia] - Combo", "cmIreliaCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useEStun", "Use e only if target can be stunned").SetValue(false));
            comboMenu.AddItem(new MenuItem("useR", "Use R in combo").SetValue(true));
            _menu.AddSubMenu(comboMenu);

            // Use combo / harass
            _menu.AddItem(new MenuItem("comboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            _menu.AddItem(new MenuItem("harassActive", "Harass").SetValue(new KeyBind('v', KeyBindType.Press)));

            // Finalize
            _menu.AddToMainMenu();
        }
    }
}
