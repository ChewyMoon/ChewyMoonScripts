using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Cya_Nerds
{
    struct WardJumpChampion
    {
        public Spell WardJumpSpell;
        public string ChampionName;

        public WardJumpChampion(string champName, Spell wardJumpSpell)
        {
            ChampionName = champName;
            WardJumpSpell = wardJumpSpell;           
        }
    }

    class Program
    {
        public static List<WardJumpChampion> WardJumpChampions;
        private static WardJumpChampion plugin;
        private static int _lastWardPlacedT;
        private static Menu _menu;

        private static bool WardJump
        {
            get { return _menu.Item("wardJump").GetValue<KeyBind>().Active; }
        }

        static void Main(string[] args)
        {
            WardJumpChampions = new List<WardJumpChampion>
            {
                new WardJumpChampion("LeeSin", new Spell(SpellSlot.W, 700)),
                new WardJumpChampion("Katarina", new Spell(SpellSlot.E, 700)),
                new WardJumpChampion("Jax", new Spell(SpellSlot.Q, 700))
            };

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            var loaded = false;
            foreach (var champ in WardJumpChampions.Where(champ => champ.ChampionName == ObjectManager.Player.ChampionName))
            {
                plugin = champ;
                loaded = true;
                break;
            }

            if (!loaded)
                return;

            _menu = new Menu("Cya Nerds", "cmCyaNerds", true);
            _menu.AddItem(new MenuItem("maxWardJump", "Jump to max range").SetValue(true));
            _menu.AddItem(new MenuItem("jumpRange", "Existing Obj Range").SetValue(new Slider(250, 0, 700)));
            _menu.AddItem(new MenuItem("wardJump", "Ward Jump").SetValue(new KeyBind('t', KeyBindType.Press)));
            _menu.AddToMainMenu();
      
            GameObject.OnCreate += GameObjectOnOnCreate;
            Game.OnGameUpdate += GameOnOnGameUpdate;
            Game.PrintChat("Cya Nerds by ChewyMoon loaded.");

        }

        private static void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            // Not sure if needed
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            // Jump to Minion, Hero & ward

            if(!plugin.WardJumpSpell.IsReady() || !WardJump)
                return;

            var jumpRange = _menu.Item("jumpRange").GetValue<Slider>().Value;

            foreach (
                var ward in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.Name.ToUpper().Contains("WARD"))
                        .Where(x => x.Distance(Game.CursorPos) < jumpRange))
            {
                plugin.WardJumpSpell.CastOnUnit(ward);
                return;
            }

            foreach (
                var hero in
                    ObjectManager.Get<Obj_AI_Hero>().Where(x => !x.IsDead).Where(x => x.Distance(Game.CursorPos) < jumpRange))
            {
                plugin.WardJumpSpell.CastOnUnit(hero);
                return;
            }

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Distance(Game.CursorPos) < jumpRange))
            {
                plugin.WardJumpSpell.CastOnUnit(minion);
                return;
            }

            //now we have to place ward :<
            //if (Environment.TickCount < lastWardPlacedT + 1000) return;
            var wardSlot = Items.GetWardSlot();
            if (!Items.CanUseItem((int) wardSlot.Id))
                return;

            var placeAtMaxRange = _menu.Item("maxWardJump").GetValue<bool>();
            var pos = Game.CursorPos;
            var range = plugin.WardJumpSpell.Range;

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
