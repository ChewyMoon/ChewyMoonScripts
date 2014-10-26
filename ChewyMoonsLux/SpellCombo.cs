#region

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Collision = LeagueSharp.Common.Collision;

#endregion

namespace ChewyMoonsLux
{
    public class SpellCombo
    {
        public static bool AnalyzeQ(PredictionInput input, PredictionOutput output)
        {
            var posList = new List<Vector3> { ObjectManager.Player.ServerPosition, output.CastPosition };
            var collision = Collision.GetCollision(posList, input);
            var minions = collision.Count(collisionObj => collisionObj.IsMinion);
            return minions > 1;
        }

        public static void CastQ(Obj_AI_Hero target)
        {
            Console.Clear();

            var prediction = ChewyMoonsLux.Q.GetPrediction(target, true);
            var minions = prediction.CollisionObjects.Count(thing => thing.IsMinion);

            if (ChewyMoonsLux.Debug)
            {
                Console.WriteLine("Minions: {0}\nToo Many: {1}", minions, minions > 1);
            }

            if (minions > 1) return;

            ChewyMoonsLux.Q.Cast(prediction.CastPosition, ChewyMoonsLux.PacketCast);
        }
    }
}