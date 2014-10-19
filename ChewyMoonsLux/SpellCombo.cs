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

        public static bool AnalyzeQ(PredictionOutput output)
        {
            var minions = output.CollisionObjects.Count(@object => @object.IsMinion);
            return minions > 1;
        }
    }
}