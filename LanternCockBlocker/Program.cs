#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace LanternCockBlocker
{
    internal class Program
    {
        private static Menu _menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            _menu = new Menu("Lantern Blocker", "cmLCB");
            _menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(new KeyBind(32, KeyBindType.Press)));
            _menu.AddItem(new MenuItem("delay", "Delay(MS)").SetValue(new Slider(150, 0, 2000)));
            _menu.AddToMainMenu();

            Game.PrintChat("Lantern Cock Blocker by ChewyMoon loaded!");

            GameObject.OnCreate += GameObject_OnCreate;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name != "ThreshLantern" || sender.IsAlly || !_menu.Item("enabled").GetValue<bool>())
            {
                return;
            }

            if (sender.Position.Distance(ObjectManager.Player.ServerPosition) > 600)
            {
                return;
            }

            Utility.DelayAction.Add(
                _menu.Item("delay").GetValue<Slider>().Value, delegate
                {
                    var ward = Items.GetWardSlot();

                    if (Items.CanUseItem((int) ward.Id))
                    {
                        Items.UseItem((int) ward.Id, sender.Position);
                    }
                });
        }
    }
}