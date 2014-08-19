using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ChewyMoonsIrelia
{
    class Program
    {
        private const string ChampName = "Irelia";

        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;

// ReSharper disable InconsistentNaming
        private static Spell Q, W, E, R;
// ReSharper restore InconsistentNaming

        // Irelia Ultimate stuff.
        private static bool _hasToFire = false;
        private static int _charges = 0;

        private const bool _packetCast = false;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != ChampName)
                return;

            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, ObjectManager.Player.AttackRange); // So confused.
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);

            //Q.SetSkillshot(0.25f, 75f, 1500f, false, Prediction.SkillshotType.SkillshotLine);
            //E.SetSkillshot(0.15f, 75f, 1500f, false, Prediction.SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.15f, 80f, 1500f, false, Prediction.SkillshotType.SkillshotLine);

            SetupMenu();

            Updater.CheckForUpdates();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += ObjAiBaseOnOnProcessSpellCast;
        }

        private static void ObjAiBaseOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!_menu.Item("interruptUlts").GetValue<bool>()) return;

            String[] spellsToInterrupt =
            {
                "AbsoluteZero",
                "AlZaharNetherGrasp",
                "CaitlynAceintheHole",
                "Crowstorm",
                "DrainChannel",
                "FallenOne",
                "GalioIdolOfDurand",
                "InfiniteDuress",
                "KatarinaR",
                "MissFortuneBulletTime",
                "Teleport",
                "Pantheon_GrandSkyfall_Jump",
                "ShenStandUnited",
                "UrgotSwap2"
            };

            var spellName = args.SData.Name;
            var target = sender;

            foreach (var spell in spellsToInterrupt.Where(spell => spell == spellName))
            {
                if (_menu.Item("interruptQE").GetValue<bool>())
                {
                    if (!CanStunTarget(target)) continue;
                    if (Q.IsReady()) Q.Cast(target, _packetCast);
                    if (E.IsReady()) E.Cast(target, _packetCast);
                }
                else
                {
                    if (!CanStunTarget(target)) continue;
                    if (E.IsReady()) E.Cast(target, _packetCast);
                }
            }

        }


        static void Game_OnGameUpdate(EventArgs args)
        {
            //_packetCast = _menu.Item("packetCast").GetValue<bool>();

            FireCharges();

            if (!Orbwalking.CanMove(100)) return;

            if (_menu.Item("comboActive").GetValue<KeyBind>().Active && !ObjectManager.Player.IsDead)
            {
                Combo();
            }
        }

        private static void FireCharges()
        {
            if (!_hasToFire) return;

            R.Cast(SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical), _packetCast); //Dunnno
            _charges -= 1;
            _hasToFire = _charges != 0;
        }

        private static void Combo()
        {
            // Simple combo q -> w -> e -> r
            Game.PrintChat("combo called");
            var useQ = _menu.Item("useQ").GetValue<bool>();
            var useW = _menu.Item("useW").GetValue<bool>();
            var useE = _menu.Item("useE").GetValue<bool>();
            var useR = _menu.Item("useR").GetValue<bool>();
            var useEStun = _menu.Item("useEStun").GetValue<bool>();
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            
            if (target == null) return;

            if (useW && W.IsReady())
            {
                W.Cast();
            }

            // follow up with q
            if (useQ && Q.IsReady())
            {
                Q.Cast(target, _packetCast);
            }

            // stunerino
            if (useE && E.IsReady())
            {
                if (useEStun)
                {
                    if (CanStunTarget(target))
                    {
                        E.Cast(target, _packetCast);
                    }
                }
                else
                {
                    E.Cast(target, _packetCast);
                }
            }
            
            // Resharper did this, IDK if it works.
            // Original code:  if (useR && R.IsReady() && !hasToFire)
            if (!useR || !R.IsReady() || _hasToFire) return;
            _hasToFire = true;
            _charges = 4;
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
            
            //Misc
            var miscMenu = new Menu("[ChewyMoon's Irelia - Misc", "cmIreliaMisc");
            miscMenu.AddItem(new MenuItem("interruptUlts", "Interrupt ults with E").SetValue(true));
            miscMenu.AddItem(new MenuItem("interruptQE", "Q + E to interrupt if not in range").SetValue(true));
            //miscMenu.AddItem(new MenuItem("packetCast", "Use packets to cast spells").SetValue(false)); //PACKETS BUGGY AF
            _menu.AddSubMenu(miscMenu);

            // Use combo
            _menu.AddItem(new MenuItem("comboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Finalize
            _menu.AddToMainMenu();
        }
    }
}
