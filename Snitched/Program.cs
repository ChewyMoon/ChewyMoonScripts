using System;
using System.Collections.Generic;
using System.Linq;
using Evade;
using LeagueSharp;
using LeagueSharp.Common;

namespace Snitched
{
    class Program
    {

        private static List<Spell> Spells { get; set; }
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Menu Menu { get; set; }

        private static Obj_AI_Minion Baron { get; set; }
        private static Obj_AI_Minion Dragon { get; set; }

        private static List<Obj_AI_Minion> Blues { get; set; }
        private static List<Obj_AI_Minion> Reds { get; set; } 

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            if (SpellDatabase.Spells.All(x => x.ChampionName != Player.ChampionName))
            {
                return;
            }

            Spells = new List<Spell>();

            foreach (var skillshot in SpellDatabase.Spells.Where(x => x.ChampionName == Player.ChampionName))
            {
                var spell = new Spell(skillshot.Slot, skillshot.Range);

                //Game.PrintChat("{0}: Delay: {1} | Type: {2}", skillshot.Slot, (float) skillshot.Delay / 1000, skillshot.Type);

                // Convert Evade skillshot type to common type
                var type = SkillshotType.SkillshotLine;
                switch (skillshot.Type)
                {
                    case SkillShotType.SkillshotCircle:
                        type = SkillshotType.SkillshotCircle;
                        break;

                    case SkillShotType.SkillshotMissileLine:
                    case SkillShotType.SkillshotLine:
                        type = SkillshotType.SkillshotLine;
                        break;

                    case SkillShotType.SkillshotMissileCone:
                    case SkillShotType.SkillshotCone:
                        type = SkillshotType.SkillshotCone;
                        break;
                }

                spell.SetSkillshot(skillshot.Delay / 1000f, skillshot.Radius, skillshot.MissileSpeed, skillshot.CollisionObjects.Contains(CollisionObjectTypes.Champions), type);
                Spells.Add(spell);
            }

            Blues = new List<Obj_AI_Minion>();
            Reds = new List<Obj_AI_Minion>();

            CreateMenu();

            GameObject.OnCreate += GameObjectOnOnCreate;
            GameObject.OnDelete += GameObjectOnOnDelete;
            Game.OnUpdate += Game_OnGameUpdate;

            Game.PrintChat("<font color=\"#7CFC00\"><b>Snitched:</b></font> Loaded");
        }


        private static void GameObjectOnOnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion))
            {
                return;
            }

            var obj = (Obj_AI_Minion)sender;
            var n = obj.BaseSkinName;

            if (n == "SRU_Baron")
            {
                Baron = null;
                return;
            }

            if (n == "SRU_Dragon")
            {
                Dragon = null;
                return;
            }

            if (n == "SRU_Blue")
            {
                Blues.RemoveAll(x => x.NetworkId == obj.NetworkId);
                return;
            }

            if (n == "SRU_Red")
            {
                Reds.RemoveAll(x => x.NetworkId == obj.NetworkId);
            }
        }

        private static void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion))
            {
                return;
            }

            var obj = (Obj_AI_Minion) sender;
            var n = obj.BaseSkinName;

            if (n == "SRU_Baron")
            {
                Baron = (Obj_AI_Minion) sender;
                return;
            }

            if (n == "SRU_Dragon")
            {
                Dragon = (Obj_AI_Minion) sender;
                return;
            }

            if (n == "SRU_Blue")
            {
                Blues.Add((Obj_AI_Minion) sender);
                return;
            }

            if (n == "SRU_Red")
            {
                Reds.Add((Obj_AI_Minion) sender);
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            // Update the notification
            NotificationHandler.Update();

            if (!(Menu.Item("Enabled").IsActive() || Menu.Item("EnabledKeybind").IsActive()))
            {
                return;
            }

            // Sort the spells
            var index = Menu.Item("SortRule").GetValue<StringList>().SelectedValue;
            switch (index)
            {
                case "Most Damage":
                    Spells = Spells.OrderByDescending(x => x.GetDamage(Player)).ToList();
                    break;

                case "Least Damage":
                    Spells = Spells.OrderBy(x => x.GetDamage(Player)).ToList();
                    break;

                case "Biggest Range":
                    Spells = Spells.OrderByDescending(x => x.Range).ToList();
                    break;

                case "Smallest Range":
                    Spells = Spells.OrderBy(x => x.Range).ToList();
                    break;

                case "Smallest Cast Time":
                    Spells = Spells.OrderBy(x => x.Delay).ToList();
                    break;
            }

            // Get baron
            if (Baron != null && Baron.IsValid && Baron.Health > 0)
            {
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(Baron, x.Range) && SkillEnabled("Steal", x.Slot)))
                {
                    var time = (1000 * Player.Distance(Baron) / spell.Speed) + spell.Delay * 1000 + Game.Ping / 2f;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(Baron, (int)time);

                    if (!(spell.GetDamage(Baron) > Baron.Health))
                    {
                        continue;
                    }

                    spell.Cast(Baron.ServerPosition);
                    break;
                }
            }

            // Get dragon
            if (Dragon != null && Dragon.IsValid && Dragon.Health > 0)
            {
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(Dragon, x.Range) && SkillEnabled("Steal", x.Slot)))
                {
                    var time = (1000 * Player.Distance(Dragon) / spell.Speed) + spell.Delay * 1000 + Game.Ping / 2f;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(Dragon, (int)time);

                    if (!(spell.GetDamage(Dragon) > Dragon.Health))
                    {
                        continue;
                    }

                    spell.Cast(Dragon.ServerPosition);
                    break;
                }
            }

            // Get Blue
            foreach (var blue in Blues.Where(x => x.IsValid && x.Health > 0))
            {
                var blue1 = blue;
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(blue1, x.Range) && SkillEnabled("Buff", x.Slot)))
                {
                    var time = (1000 * Player.Distance(blue) / spell.Speed) + spell.Delay * 1000 + Game.Ping / 2f;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(blue, (int)time);

                    if (!(spell.GetDamage(blue) > blue.Health))
                    {
                        continue;
                    }

                    spell.Cast(blue.ServerPosition);
                    break;
                }
            }

            // Get Red
            foreach (var red in Reds.Where(x => x.IsValid && x.Health > 0))
            {
                var red1 = red;
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(red1, x.Range) && SkillEnabled("Buff", x.Slot)))
                {
                    var time = (1000 * Player.Distance(red) / spell.Speed) + spell.Delay * 1000 + Game.Ping / 2f;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(red, (int)time);

                    if (!(spell.GetDamage(red) > red.Health))
                    {
                        continue;
                    }

                    spell.Cast(red.ServerPosition);
                    break;
                }
            }

            // Kill SECURE
            foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget()))
            {
                var hero1 = hero;
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(hero1, x.Range) && SkillEnabled("KS", x.Slot)))
                {
                    var time = (1000 * Player.Distance(hero) / spell.Speed) + spell.Delay * 1000 + Game.Ping / 2f;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(hero, (int)time);

                    if (!(spell.GetDamage(hero) > hero.Health))
                    {
                        continue;
                    }

                    spell.Cast(hero.ServerPosition);
                    break;
                }
            }
        }

        private static bool SkillEnabled(string mode, SpellSlot slot)
        {
            return Menu.Item(mode + slot).GetValue<bool>();
        }

        private static void CreateMenu()
        {
            Menu = new Menu("Snitched", "cmSnitched" + Player.ChampionName, true);

            // Dragion and baron stealer
            var spellsMenu = new Menu("Drag/Baron Stealer", "epicStealz");
            foreach (var name in Spells)
            {
                spellsMenu.AddItem(new MenuItem("Steal" + name.Slot, "Use " + name.Slot).SetValue(true));
            }
            Menu.AddSubMenu(spellsMenu);

            // Buff stealing
            var buffsMenu = new Menu("Buffs", "epicBuffzStealz");
            foreach (var name in Spells)
            {
                buffsMenu.AddItem(new MenuItem("Buff" + name.Slot, "Use " + name.Slot).SetValue(name.Slot != SpellSlot.R));
            }
            Menu.AddSubMenu(buffsMenu);

            // Kill Secure menu
            var ksMenu = new Menu("Kill Secure", "epicKillzStealz");
            foreach (var name in Spells)
            {
                ksMenu.AddItem(new MenuItem("KS" + name.Slot, "Use " + name.Slot).SetValue(name.Slot != SpellSlot.R));
            }
            Menu.AddSubMenu(ksMenu);

            Menu.AddItem(
                new MenuItem("SortRule", "Sort Spell Priority").SetValue(
                    new StringList(new[] { "Most Damage", "Least Damage", "Biggest Range", "Smallest Range", "Smallest Cast Time" })));

            Menu.AddItem(new MenuItem("Enabled", "Enabled (Toggle)").SetValue(true));
            Menu.AddItem(new MenuItem("EnabledKeybind", "Enabled (Press)").SetValue(new KeyBind(84, KeyBindType.Press)));

            Menu.AddToMainMenu();
        }
    }
}
