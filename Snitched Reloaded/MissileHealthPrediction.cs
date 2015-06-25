using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Snitched_Reloaded
{
    /// <summary>
    ///     Gets the damage done by missiles on a unit.
    /// </summary>
    class MissileHealthPrediction
    {
        public static List<Obj_SpellMissile> Missiles = new List<Obj_SpellMissile>();  

        public static void Init()
        {
            GameObject.OnCreate += SpellMissile_OnCreate;
            GameObject.OnDelete += SpellMissile_OnDelete;
            Game.OnUpdate += GameOnOnUpdate;
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            Missiles.RemoveAll(x => !x.IsValid);
        }

        private static void SpellMissile_OnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_SpellMissile))
            {
                return;
            }

            Missiles.RemoveAll(x => x.NetworkId == sender.NetworkId);
        }

        static void SpellMissile_OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_SpellMissile))
            {
                return;
            }

            var missile = (Obj_SpellMissile) sender;
            Missiles.Add(missile);       
        }

        /// <summary>
        ///     Gets the predicted health of a unit.
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="time">Time in miliseconds</param>
        /// <returns>Predicted Health.</returns>
        public static float GetPredictedHealth(Obj_AI_Base target, float time)
        {
            var health = target.Health;

            foreach (var missile in Missiles)
            {
                var missileArriveTime = 1000 * missile.Position.Distance(target.ServerPosition) / missile.SData.MissileSpeed;
                var missileRectangle = new Geometry.Polygon.Rectangle(missile.StartPosition, missile.EndPosition, missile.SData.LineWidth);

                if (missileRectangle.IsInside(target) && missileArriveTime >= time)
                {
                    health -= (float) missile.SpellCaster.GetSpellDamage(target, missile.SData.Name);
                }
            }

            return health;
        }
    }
}
