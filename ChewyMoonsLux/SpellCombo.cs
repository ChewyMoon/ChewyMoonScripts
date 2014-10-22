using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChewyMoonsLux
{
    public class SpellCombo
    {
        public static bool ContainsPassive(Dictionary<Obj_AI_Hero, bool> dictionary, string baseSkinName)
        {
            return dictionary.Any(pair => pair.Key.BaseSkinName == baseSkinName && pair.Value);
        }

        public static bool AnalyzeQ(PredictionInput input, PredictionOutput output)
        {
            var posList = new List<SharpDX.Vector3> { ObjectManager.Player.ServerPosition, output.CastPosition };
            var collision = Collision.GetCollision(posList, input);
            var minions = collision.Count(collisionObj => collisionObj.IsMinion);
            Console.WriteLine("Minions: {0}", minions);
            return minions > 1;
        }

        public static void CastQ(Obj_AI_Hero target)
        {
            var prediction = ChewyMoonsLux.Q.GetPrediction(target, true);
            var minions = prediction.CollisionObjects.Count(thing => thing.IsMinion);

            Console.WriteLine("[{0}] Minions: {1}", DateTime.Now, minions);

            if (minions > 1) return;
            Console.WriteLine("[{0}] Too many minions!", DateTime.Now);

            ChewyMoonsLux.Q.Cast(prediction.CastPosition, ChewyMoonsLux.PacketCast);
        }
    }
}