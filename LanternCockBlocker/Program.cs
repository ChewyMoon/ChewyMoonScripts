using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace LanternCockBlocker
{
    class Program
    {
        private static Menu menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            menu = new Menu("Lantern Blocker", "cmLCB");
            menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(new KeyBind(32, KeyBindType.Press)));
            menu.AddToMainMenu();

            Game.PrintChat("Lantern Cock Blocker by ChewyMoon loaded!");

            GameObject.OnCreate += GameObject_OnCreate;
        }

        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name != "ThreshLantern" || sender.IsAlly || !menu.Item("enabled").GetValue<bool>())
                return;

            if (sender.Position.Distance(ObjectManager.Player.ServerPosition) > 600)
                return;

            var ward = Items.GetWardSlot();

            if (Items.CanUseItem((int)ward.Id))
            {
                Items.UseItem((int) ward.Id, sender.Position);
            }
        }
    }
}
