using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Microsoft.Win32.SafeHandles;
using SharpDX;
using Color = System.Drawing.Color;

namespace Snitched_Reloaded
{
    class Program
    {
        static readonly List<SpellWrapper> Spells = new List<SpellWrapper>();

        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static Menu Menu { get; set; }

        static Notification OnNotification { get; set; }

        static List<Obj_AI_Minion> BlueBuffs { get; set; }
        static List<Obj_AI_Minion> RedBuffs { get; set; }
        static Obj_AI_Minion Dragon { get; set; }
        static Obj_AI_Minion Baron { get; set; }

        static List<string> CastedSpellsNames { get; set; }
        static bool Enabled { get
        {
            return Menu.Item("EnabledToggle").IsActive() || Menu.Item("EnabledKeyBind").IsActive();
        } }

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            OnNotification = new Notification("Snitched Reloaded: On")
            {
                TextColor = new ColorBGRA(Color.Turquoise.R, Color.Turquoise.G, Color.Turquoise.B, Color.Turquoise.A),
                BorderColor = new ColorBGRA(Color.Turquoise.R, Color.Turquoise.G, Color.Turquoise.B, Color.Turquoise.A)
            };
            Notifications.AddNotification(OnNotification);

            BlueBuffs = new List<Obj_AI_Minion>();
            RedBuffs = new List<Obj_AI_Minion>();
            CastedSpellsNames = new List<string>();

            // Create menu
            Menu = new Menu("Snitched: RELOADED", "snitchedreload", true);

            // Buffs
            var buffMenu = new Menu("Buffs", "buffs");
            buffMenu.AddItem(new MenuItem("StealAllyBlue", "Steal Ally Blue Buff").SetValue(false));
            buffMenu.AddItem(new MenuItem("StealAllyRed", "Steal Ally Red Buff").SetValue(false));
            buffMenu.AddItem(new MenuItem("StealEnemyBlue", "Steal Enemy Blue Buff").SetValue(false));
            buffMenu.AddItem(new MenuItem("StealEnemyRed", "Steal Enemy Red Buff").SetValue(false));
            buffMenu.AddItem(new MenuItem("penissXD", " "));
            Menu.AddSubMenu(buffMenu);

            // Objectives
            var objectiveMenu = new Menu("Objectives", "objectives");
            objectiveMenu.AddItem(new MenuItem("StealDragon", "Steal Dragon").SetValue(true));
            objectiveMenu.AddItem(new MenuItem("StealBaron", "Steal Baron").SetValue(true));
            buffMenu.AddItem(new MenuItem("penisx2 XD", " "));
            Menu.AddSubMenu(objectiveMenu);

            // Kill Stealing
            var ksMenu = new Menu("Kill Stealing", "ks");
            ksMenu.AddItem(new MenuItem("StealKills", "Steal Kills").SetValue(true));
            buffMenu.AddItem(new MenuItem("Line break pls", " "));
            Menu.AddSubMenu(ksMenu);

            // Drawing 
            var drawMenu = new Menu("Drawing", "draw");
            drawMenu.AddItem(new MenuItem("DrawStatus", "Draw On").SetValue(true));
            drawMenu.Item("DrawStatus").ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                if (eventArgs.GetNewValue<bool>())
                {
                    Notifications.AddNotification(OnNotification);
                }
                else
                {
                    Notifications.RemoveNotification(OnNotification);
                }
            };
            drawMenu.AddItem(new MenuItem("DrawSpell", "Draw Spell Prediction").SetValue(true));
           // drawMenu.AddItem(new MenuItem("DrawStealInfo", "Draw Steal Information").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            // MISC
            var miscMenu = new Menu("Misc", "misc");
            miscMenu.AddItem(
                new MenuItem("TimeTooLong", "Don't cast spell if time > (MS)").SetValue(new Slider(2500, 0, 10000)));

            Menu.AddItem(new MenuItem("EnabledToggle", "Enabled (Toggle)").SetValue(true));
            Menu.AddItem(new MenuItem("EnabledKeyBind", "Enabled (Press)").SetValue(new KeyBind(90, KeyBindType.Press)));
            Menu.AddSubMenu(miscMenu);

            Menu.AddToMainMenu();


            // Load all champion data
            foreach (var spell in Player.Spellbook.Spells.Where(x => x.Slot == SpellSlot.Q || x.Slot == SpellSlot.W || x.Slot == SpellSlot.E || x.Slot == SpellSlot.R))
            {
                Game.PrintChat("{0}: {1}", spell.Slot, spell.SData.TargettingType);
                var spellData = spell.SData;

                if (spellData.TargettingType == SpellDataTargetType.Self ||
                    spellData.TargettingType == SpellDataTargetType.LocationTunnel ||
                    spellData.TargettingType == SpellDataTargetType.SelfAoe ||
                    spellData.TargettingType == SpellDataTargetType.SelfAndUnit)
                {
                    continue;
                }

                // Check if spell does damage
                if (Math.Abs(Player.GetSpellDamage(Player, spell.Slot)) < 1)
                {
                    continue;
                }

                Menu.SubMenu("buffs").AddItem(new MenuItem("Buffs" + spell.Slot, "Use " + spell.Slot).SetValue(true));
                Menu.SubMenu("objectives").AddItem(new MenuItem("Objective" + spell.Slot, "Use " + spell.Slot).SetValue(true));
                Menu.SubMenu("ks").AddItem(new MenuItem("KillSteal" + spell.Slot, "Use " + spell.Slot).SetValue(spell.Slot != SpellSlot.R));

                Spells.Add(new SpellWrapper(spell.Slot, spellData));
            }

            Game.OnUpdate += GameOnOnUpdate;
            GameObject.OnCreate += GameObjectOnCreate;
            GameObject.OnDelete += GameObjectOnOnDelete;
            Drawing.OnDraw += DrawingOnOnDraw;
            MissileHealthPrediction.Init();

            Game.PrintChat("<font color=\"#7CFC00\"><b>Snitched RELOADED:</b></font> Loaded");
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (!Menu.Item("DrawSpell").IsActive())
            {
                return;
            }

            var rectangle = new Geometry.Polygon.Rectangle(new Vector3(), new Vector3(), 0);
            foreach (var missile in ObjectManager.Get<Obj_SpellMissile>().Where(x => x.SpellCaster.IsMe /*&& CastedSpellsNames.Any(y => y == x.SData.Name)*/))
            {
                var missilePosition = missile.Position.To2D();
                var unitPosition = missile.StartPosition.To2D();
                var endPos = missile.EndPosition.To2D();

                //Calculate the real end Point:
                var direction = (endPos - unitPosition).Normalized();
                if (unitPosition.Distance(endPos) > missile.SData.CastRange)
                {
                    endPos = unitPosition + direction * missile.SData.CastRange;
                }

                rectangle.Start = unitPosition;
                rectangle.End = endPos;
                rectangle.Width = missile.SData.LineWidth;

                rectangle.UpdatePolygon();
                rectangle.Draw(Color.LightYellow);

                // Draw Line
                var start = missilePosition + missile.SData.LineWidth * rectangle.Direction.Perpendicular();
                var end = missilePosition - missile.SData.LineWidth * rectangle.Direction.Perpendicular();

                Drawing.DrawLine(Drawing.WorldToScreen(start.To3D()), Drawing.WorldToScreen(end.To3D()), 3, Color.Chartreuse);
            }

        }

        private static void GameObjectOnOnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion))
            {
                return;
            }

            var obj = (Obj_AI_Minion)sender;

            switch (obj.BaseSkinName.ToLower())
            {
                case "sru_baron":
                    Baron = null;
                    return;
                case "sru_dragon":
                    Dragon = null;
                    return;
                case "sru_blue":
                    BlueBuffs.RemoveAll(x => x.NetworkId == obj.NetworkId);
                    return;
                case "sru_red":
                    RedBuffs.RemoveAll(x => x.NetworkId == obj.NetworkId);
                    return;
            }
        }

        static void GameObjectOnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion))
            {
                return;
            }

            var obj = (Obj_AI_Minion)sender;

            switch (obj.BaseSkinName.ToLower())
            {
                case "sru_baron":
                    Baron = (Obj_AI_Minion)sender;
                    return;
                case "sru_dragon":
                    Dragon = (Obj_AI_Minion)sender;
                    return;
                case "sru_blue":
                    BlueBuffs.Add((Obj_AI_Minion)sender);
                    return;
                case "sru_red":
                    RedBuffs.Add((Obj_AI_Minion)sender);
                    break;
            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (!Enabled)
            {
                OnNotification.Text = "Snitched Reloaded: Off";
                return;
            }

            OnNotification.Text = "Snitched Reloaded: On";

            if (Dragon != null && Menu.Item("StealDragon").IsActive())
            {
                CastSpellOn(Dragon, "Objective");
            }

            if (Baron != null && Menu.Item("StealBaron").IsActive())
            {
                CastSpellOn(Baron, "Objective");
            }

            foreach (var blue in BlueBuffs)
            {
                CastSpellOn(blue, "Buffs");
            }

            foreach (var red in RedBuffs)
            {
                CastSpellOn(red, "Buffs");
            }

            foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget()))
            {
                CastSpellOn(enemy, "KillSteal");
            }

        }

        private static void CastSpellOn(Obj_AI_Base unit, string mode)
        {
            foreach (var spell in Spells.Where(x => x.IsEnabledAndReady(mode) && Player.Distance(unit) < x.SpellData.CastRange))
            {
                var timeToArrive = (1000 * Player.Distance(unit) / spell.SpellData.MissileSpeed);

                // Spell is too far away!
                if (timeToArrive > Menu.Item("TimeTooLong").GetValue<Slider>().Value)
                {
                    continue;
                }

                var health = HealthPrediction.GetHealthPrediction(unit, (int) timeToArrive);

                if (Player.GetSpellDamage(unit, spell.Slot) > health)
                {
                    spell.CastSpell(unit);

                    if (CastedSpellsNames.Contains(spell.SpellData.Name))
                    {
                        continue;
                    }

                    CastedSpellsNames.Add(spell.SpellData.Name);

                    // Keep thread safe
                    var spell2 = spell;
                    Utility.DelayAction.Add((int) timeToArrive + Game.Ping, () => CastedSpellsNames.Remove(spell2.SpellData.Name));
                }
            }
        }
    }

    public static class SpellExtensions
    {
        public static bool IsEnabledAndReady(this SpellWrapper wrapper, string mode)
        {
            return wrapper.IsReady() && Program.Menu.Item(mode + wrapper.Slot).IsActive();
        }
    }
}
