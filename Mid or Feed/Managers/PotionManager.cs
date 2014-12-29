using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mid_or_Feed.Managers
{
    class PotionManager : Manager
    {
        private static Menu _menu;

        private const string HpPotName = "RegenerationPotion";
        private const string MpPotName = "FlaskOfCrystalWater";

        private const int Hpid = 2003;
        private const int Mpid = 2004;

        public override void Load(Menu config)
        {
            config.AddItem(new MenuItem("useHP", "Use Health Pot").SetValue(true));
            config.AddItem(new MenuItem("useHPPercent", "Health %").SetValue(new Slider(35, 1)));
            config.AddItem(new MenuItem("sseperator", "       "));
            config.AddItem(new MenuItem("useMP", "Use Mana Pot").SetValue(true));
            config.AddItem(new MenuItem("useMPPercent", "Mana %").SetValue(new Slider(35, 1)));

            _menu = config;

            Game.OnGameUpdate += GameOnOnGameUpdate;
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            var useHp = _menu.Item("useHP").GetValue<bool>();
            var useMp = _menu.Item("useMP").GetValue<bool>();

            if (useHp && ObjectManager.Player.HealthPercentage() <= _menu.Item("useHPPercent").GetValue<Slider>().Value && !HasHealthPotBuff())
            {
                // Cast health pot
                if (Items.CanUseItem(Hpid) && Items.HasItem(Hpid))
                {
                   Items.UseItem(Hpid);
                }
            }

            if (!useMp || !(ObjectManager.Player.ManaPercentage() <= _menu.Item("useMPPercent").GetValue<Slider>().Value) ||
                HasMannaPutBuff()) return;

            if (Items.CanUseItem(Mpid) && Items.HasItem(Mpid))
            {
                Items.UseItem(Mpid);
            }
        }

        private static bool HasHealthPotBuff()
        {
            return ObjectManager.Player.HasBuff(HpPotName, true);
        }

        private static bool HasMannaPutBuff()
        {
            return ObjectManager.Player.HasBuff(MpPotName, true);
        }
    }
}
