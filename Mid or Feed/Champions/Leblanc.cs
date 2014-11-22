using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Mid_or_Feed.Champions
{
    class Leblanc : Plugin
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public Leblanc()
        {
            Q = new Spell(SpellSlot.Q, 720);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R);

            W.SetSkillshot(0.5f, 200, 1200, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 100, 1750, true, SkillshotType.SkillshotLine);

            // Detect spell, and set R to the last spell casted.
            Game.OnGameSendPacket += delegate(GamePacketEventArgs args)
            {
                var pdata = args.PacketData;

                if (pdata[0] != Packet.C2S.Cast.Header)
                    return;

                var decoded = Packet.C2S.Cast.Decoded(pdata);

                if (!ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(decoded.SourceNetworkId).IsMe)
                    return;

                switch (decoded.Slot)
                {
                    case SpellSlot.Q:
                        R = Q;
                        break;
                    case SpellSlot.W:
                        R = W;
                        break;
                    case SpellSlot.E:
                        R = E;
                        break;
                }
            };

        }
    }
}
