#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Cya_Nerds
{
    internal struct WardJumpChampion
    {
        public string ChampionName;
        public Spell WardJumpSpell;

        public WardJumpChampion(string champName, Spell wardJumpSpell)
        {
            ChampionName = champName;
            WardJumpSpell = wardJumpSpell;
        }
    }

    internal class Program
    {
        public static List<WardJumpChampion> WardJumpChampions;
        private static WardJumpChampion _plugin;
        private static int _lastWardPlacedT;
        private static Menu _menu;

        private static bool WardJump
        {
            get { return _menu.Item("wardJump").GetValue<KeyBind>().Active; }
        }

        private static void Main(string[] args)
        {
            WardJumpChampions = new List<WardJumpChampion>
            {
                new WardJumpChampion("LeeSin", new Spell(SpellSlot.W, 700)),
                new WardJumpChampion("Katarina", new Spell(SpellSlot.E, 700)),
                new WardJumpChampion("Jax", new Spell(SpellSlot.Q, 700))
            };

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            var loaded = false;
            foreach (
                var champ in WardJumpChampions.Where(champ => champ.ChampionName == ObjectManager.Player.ChampionName))
            {
                _plugin = champ;
                loaded = true;
                break;
            }

            if (!loaded)
            {
                return;
            }

            _menu = new Menu("Cya Nerds", "cmCyaNerds", true);
            _menu.AddItem(new MenuItem("maxWardJump", "Jump to max range").SetValue(true));
            _menu.AddItem(new MenuItem("jumpRange", "Existing Obj Range").SetValue(new Slider(250, 0, 700)));
            _menu.AddItem(new MenuItem("wardDelay", "Ward Delay(MS)").SetValue(new Slider(3000, 0, 10 * 1000)));
            _menu.AddItem(
                new MenuItem("wardJump", "Ward Jump").SetValue(new KeyBind("t".ToCharArray()[0], KeyBindType.Press)));
            _menu.AddToMainMenu();

            GameObject.OnCreate += GameObjectOnOnCreate;
            Game.OnUpdate += GameOnOnGameUpdate;
            Game.PrintChat("Cya Nerds by ChewyMoon loaded.");
        }

        private static void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (!WardJump)
            {
                return;
            }

            if (!(sender is Obj_AI_Minion))
            {
                return;
            }

            if (!sender.Name.ToUpper().Contains("WARD"))
            {
                return;
            }

            var ward = (Obj_AI_Minion) sender;
            if (sender.Position.Distance(ObjectManager.Player.ServerPosition) <= _plugin.WardJumpSpell.Range)
            {
                _plugin.WardJumpSpell.CastOnUnit(ward);
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            // Jump to Minion, Hero & ward

            if (!_plugin.WardJumpSpell.IsReady() || !WardJump)
            {
                return;
            }

            var jumpRange = _menu.Item("jumpRange").GetValue<Slider>().Value;

            foreach (var ward in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(x => x.Name.ToUpper().Contains("WARD"))
                    .Where(x => x.Distance(Game.CursorPos) < jumpRange))
            {
                _plugin.WardJumpSpell.CastOnUnit(ward);
                return;
            }

            foreach (var hero in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => !x.IsDead)
                    .Where(x => !x.IsMe)
                    .Where(x => x.Distance(Game.CursorPos) < jumpRange))
            {
                _plugin.WardJumpSpell.CastOnUnit(hero);
                return;
            }

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Distance(Game.CursorPos) < jumpRange)
                )
            {
                _plugin.WardJumpSpell.CastOnUnit(minion);
                return;
            }

            //now we have to place ward :<
            if (Environment.TickCount < _lastWardPlacedT + _menu.Item("wardDelay").GetValue<Slider>().Value)
            {
                return;
            }

            if (!_plugin.WardJumpSpell.IsReady() || _plugin.WardJumpSpell.Instance.SData.Name == "blindmonkwtwo")
            {
                return;
            }

            var wardSlot = Items.GetWardSlot();
            if (!Items.CanUseItem((int) wardSlot.Id) || wardSlot.Stacks == 0)
            {
                return;
            }

            var placeAtMaxRange = _menu.Item("maxWardJump").GetValue<bool>();
            var pos = Game.CursorPos;
            const int range = 600; // ward range

            if (!placeAtMaxRange)
            {
                Items.UseItem((int) wardSlot.Id, Game.CursorPos);
                _lastWardPlacedT = Environment.TickCount;
                return;
            }

            // extend the mouse pos
            var placePos = ObjectManager.Player.ServerPosition.To2D().Extend(pos.To2D(), range);
            Items.UseItem((int) wardSlot.Id, placePos);
            _lastWardPlacedT = Environment.TickCount;
        }
    }
}