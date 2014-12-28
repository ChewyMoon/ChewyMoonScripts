using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mid_or_Feed.Plugins
{
    class PotionManager
    {
        private static Menu menu;

        private const string HpPotName = "RegenerationPotion";
        private const string MpPotNAme = "FlaskOfCrystalWater";

        public static void Load(Menu config)
        {
            config.AddItem(new MenuItem("useHP", "Use Health Pot").SetValue(true));
            config.AddItem(new MenuItem("useHPPercent", "Health %").SetValue(new Slider(35, 1)));
            config.AddItem(new MenuItem("sseperator", "       "));
            config.AddItem(new MenuItem("useMP", "Use Mana Pot").SetValue(true));
            config.AddItem(new MenuItem("useMPPercent", "Mana %").SetValue(new Slider(35, 1)));

            menu = config;

            Game.OnGameUpdate += GameOnOnGameUpdate;
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            if (menu.Item("useHP").GetValue<bool>() &&
                ObjectManager.Player.Health <= menu.Item("useHPPercent").GetValue<Slider>().Value && !HasHpPotBuff())
            {
                // Cast health pot
                if (Items.CanUseItem((int) ItemId.Health_Potion))
                {
                   // Items.UseItem();
                }
            }
        }

        private static bool HasHpPotBuff()
        {
            return ObjectManager.Player.HasBuff(HpPotName, true);
        }

        private static bool HasMpPotBuff()
        {
            return ObjectManager.Player.HasBuff(MpPotNAme, true);
        }
    }
}
