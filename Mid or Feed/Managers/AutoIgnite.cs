using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mid_or_Feed.Managers
{
    class AutoIgnite : Manager
    {
        private SpellSlot _igniteSlot;
        private const int IgniteRange = 600;

        private Menu _menu;

        public override void Load(Menu config)
        {
            config.AddItem(new MenuItem("igniteKill", "Use Ignite if Killable").SetValue(true));
            config.AddItem(new MenuItem("igniteKS", "Use Ignite KS").SetValue(true));

            _menu = config;
            _igniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            Game.OnGameUpdate += GameOnOnGameUpdate;
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
            var igniteKill = _menu.Item("igniteKill").GetValue<bool>();
            var igniteKS = _menu.Item("igniteKS").GetValue<bool>();

            if (!_igniteSlot.IsReady())
                return;

            if (igniteKill)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy).Where(x => x.Distance(ObjectManager.Player) <= IgniteRange).Where(enemy => ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite) > enemy.Health))
                {
                    ObjectManager.Player.Spellbook.CastSpell(_igniteSlot, enemy);
                    return;
                }
            }

            if (!igniteKS) return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy).Where(x => x.Distance(ObjectManager.Player) <= IgniteRange).Where(enemy => enemy.Health <=
                                                                                                                                                                    ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite)/5))
            {
                ObjectManager.Player.Spellbook.CastSpell(_igniteSlot, enemy);
                return;
            }
        }
    }
}
