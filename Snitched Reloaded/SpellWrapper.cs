using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Snitched_Reloaded
{
    public class SpellWrapper
    {
        public readonly SpellSlot Slot;
        public readonly SpellData SpellData;

        public SpellWrapper(SpellSlot slot, SpellData spellData)
        {
            Slot = slot;
            SpellData = spellData;
        }

        public bool IsReady()
        {
            return SpellBook.CanUseSpell(Slot) == SpellState.Ready;
        }

        public static Spellbook SpellBook
        {
            get { return ObjectManager.Player.Spellbook; }
        }

        public void CastSpell(Obj_AI_Base target)
        {
            var predictionOutput = Prediction.GetPrediction(target, SpellData.CastFrame/30, SpellData.LineWidth,
                SpellData.MissileSpeed,
                SpellData.HaveHitBone
                    ? new[] {CollisionableObjects.Heroes, CollisionableObjects.YasuoWall,}
                    : new CollisionableObjects[0]);

            if (predictionOutput == null || !predictionOutput.CastPosition.IsValid())
            {
                return;
            }

            var castPosition = predictionOutput.CastPosition;

            switch (SpellData.TargettingType)
            {
                case SpellDataTargetType.Unit:
                    SpellBook.CastSpell(Slot, target);
                    break;
                case SpellDataTargetType.LocationAoe:
                    SpellBook.CastSpell(Slot, castPosition);
                    break;
                case SpellDataTargetType.Cone:
                    SpellBook.CastSpell(Slot, castPosition);
                    break;
                case SpellDataTargetType.Location:
                    SpellBook.CastSpell(Slot, castPosition);
                    break;
                case SpellDataTargetType.Location2:
                    SpellBook.CastSpell(Slot, castPosition);
                    break;
                case SpellDataTargetType.LocationVector:
                    SpellBook.CastSpell(Slot, castPosition);
                    break;
            }
        }

        public SkillshotType GetSkillshotType()
        {
            switch (SpellData.TargettingType)
            {
                case SpellDataTargetType.LocationAoe:
                    return SkillshotType.SkillshotCircle;

                case SpellDataTargetType.Cone:
                    return SkillshotType.SkillshotCone;

                case SpellDataTargetType.Location:
                    return SkillshotType.SkillshotLine;

                case SpellDataTargetType.Location2:
                    return SkillshotType.SkillshotLine;

                case SpellDataTargetType.LocationVector:
                    return SkillshotType.SkillshotLine;
            }

            Console.WriteLine("UNKNOWN SKILLSHOT TYPE: {0}", SpellData.TargettingType);
            return default(SkillshotType);
        }

        public PredictionInput CreatePrediction(Obj_AI_Base target)
        {
            var input = new PredictionInput
            {
                Aoe = false,
                Collision = SpellData.HaveHitBone,
                Delay = SpellData.CastFrame/30,
                Radius = SpellData.LineWidth,
                Speed = SpellData.MissileSpeed,
                Type = GetSkillshotType(),
                Range = SpellData.CastRange,
                Unit = target,
                RangeCheckFrom = ObjectManager.Player.ServerPosition,
                From = ObjectManager.Player.ServerPosition,
                CollisionObjects = SpellData.HaveHitBone ? new []{CollisionableObjects.Heroes, CollisionableObjects.YasuoWall, } : new CollisionableObjects[0],
                UseBoundingRadius = true,

            };

            return input;
        }
    }
}