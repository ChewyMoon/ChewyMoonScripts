using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ChewyMoonsLux
{
    class ChewyMoonsLux
    {
        private Spell Q, W, E, R;
        public void OnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
        }
    }
}
