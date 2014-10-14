using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoon.Utility
{
    public class SpellHelper
    {
        public static Spell CreateTargettedSpell(SpellSlot slot)
        {
            var sdata = ObjectManager.Player.Spellbook.GetSpell(slot).SData;

            var speed = sdata.MissileSpeed;
            var delay = sdata.SpellCastTime;
            var range = sdata.CastRange[0];

            var spell = new Spell(slot, range);
            spell.SetTargetted(delay, speed);

            return spell;
        }

        public static Spell CreateSkillshotSpell(SpellSlot slot, SkillshotType type, bool collision)
        {
            var sdata = ObjectManager.Player.Spellbook.GetSpell(slot).SData;

            var range = sdata.CastRange[0];
            var delay = sdata.SpellCastTime;
            var width = sdata.LineWidth;
            var speed = sdata.MissileSpeed;

            var spell = new Spell(slot, range);
            spell.SetSkillshot(delay, width, speed, collision, type);

            return spell;
        }
    }
}
