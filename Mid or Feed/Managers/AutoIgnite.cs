#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Mid_or_Feed.Managers
{
    internal class AutoIgnite : Manager
    {
        private const int IgniteRange = 600;
        private SpellSlot _igniteSlot;
        private Menu _menu;

        public override void Load(Menu config)
        {
            config.AddItem(new MenuItem("igniteKill", "Use Ignite if Killable").SetValue(true));
            config.AddItem(new MenuItem("igniteKS", "Use Ignite KS").SetValue(true));

            _menu = config;
            _igniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            Game.OnUpdate += GameOnOnGameUpdate;
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
            var igniteKill = _menu.Item("igniteKill").GetValue<bool>();
            var igniteKs = _menu.Item("igniteKS").GetValue<bool>();

            if (!_igniteSlot.IsReady())
            {
                return;
            }

            if (igniteKill)
            {
                var igniteKillableEnemy =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsEnemy)
                        .Where(x => !x.IsDead)
                        .Where(x => x.Distance(ObjectManager.Player) <= IgniteRange)
                        .FirstOrDefault(
                            x => ObjectManager.Player.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite) > x.Health);

                if (igniteKillableEnemy.IsValidTarget())
                {
                    ObjectManager.Player.Spellbook.CastSpell(_igniteSlot, igniteKillableEnemy);
                }
            }

            if (!igniteKs)
            {
                return;
            }

            var enemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsEnemy)
                    .Where(x => x.Distance(ObjectManager.Player) <= IgniteRange)
                    .FirstOrDefault(
                        x => x.Health <= ObjectManager.Player.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite) / 5);

            if (enemy.IsValidTarget())
            {
                ObjectManager.Player.Spellbook.CastSpell(_igniteSlot, enemy);
            }
        }
    }
}