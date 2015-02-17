using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Snitched.Spells;

namespace Snitched
{
    class Program
    {
        //TODO: Add notifications

        private static List<Spell> Spells { get; set; }
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Menu Menu { get; set; }

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

            CreateMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;

            Game.PrintChat("<font color=\"#7CFC00\"><b>Snitched:</b></font> Loaded");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
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

            // TODO: Cache baron & drag to increase preformance
            // Get baron
            var baron = ObjectManager.Get<Obj_AI_Minion>().Where(x => !x.IsDead).FirstOrDefault(x => x.BaseSkinName == "SRU_Baron");
            if (baron != null)
            {
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(baron, x.Range) && SkillEnabled("Steal", x.Slot)))
                {
                    var time = (Player.Distance(baron) / spell.Speed) + spell.Delay * 1000;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(baron, (int) time);

                    if (!(spell.GetDamage(baron) > healthPrediciton))
                    {
                        continue;
                    }

                    spell.Cast(baron.ServerPosition);
                    break;
                }
            }

            // Get dragon
            var dragon = ObjectManager.Get<Obj_AI_Minion>().Where(x => !x.IsDead).FirstOrDefault(x => x.BaseSkinName == "SRU_Dragon");
            if (dragon != null)
            {
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(dragon, x.Range) && SkillEnabled("Steal", x.Slot)))
                {
                    var time = (Player.Distance(dragon) / spell.Speed) + spell.Delay * 1000;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(dragon, (int)time);

                    if (!(spell.GetDamage(dragon) > healthPrediciton))
                    {
                        continue;
                    }

                    spell.Cast(dragon.ServerPosition);
                    break;
                }
            }

            // Get Blue
            var blues = ObjectManager.Get<Obj_AI_Minion>()
                .Where(x => !x.IsDead)
                .Where(x => x.BaseSkinName == "SRU_Blue");

            foreach (var blue in blues)
            {
                var blue1 = blue;
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(blue1, x.Range) && SkillEnabled("Buff", x.Slot)))
                {
                    var time = (Player.Distance(blue) / spell.Speed) + spell.Delay * 1000;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(blue, (int)time);

                    if (!(spell.GetDamage(blue) > healthPrediciton))
                    {
                        continue;
                    }

                    spell.Cast(blue.ServerPosition);
                    break;
                }
            }

            // Get Red
            var reds = ObjectManager.Get<Obj_AI_Minion>()
                .Where(x => !x.IsDead)
                .Where(x => x.BaseSkinName == "SRU_Red");

            foreach (var red in reds)
            {
                var red1 = red;
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(red1, x.Range) && SkillEnabled("Buff", x.Slot)))
                {
                    var time = (Player.Distance(red) / spell.Speed) + spell.Delay * 1000;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(red, (int)time);

                    if (!(spell.GetDamage(red) > healthPrediciton))
                    {
                        continue;
                    }

                    spell.Cast(red.ServerPosition);
                    break;
                }
            }

            // Kill SECURE
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget()))
            {
                var hero1 = hero;
                foreach (var spell in Spells.Where(x => x.IsReady() && x.IsInRange(hero1, x.Range) && SkillEnabled("KS", x.Slot)))
                {
                    var time = (Player.Distance(hero) / spell.Speed) + spell.Delay * 1000;
                    var healthPrediciton = HealthPrediction.GetHealthPrediction(hero, (int)time);

                    if (!(spell.GetDamage(hero) > healthPrediciton))
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
            Menu = new Menu("OneKeyBuffStealerAIO", "cmOneKeyBuffStealerAIO" + Player.ChampionName, true);

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
