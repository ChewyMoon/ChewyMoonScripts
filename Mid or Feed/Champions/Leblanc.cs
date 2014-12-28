#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Mid_or_Feed.Champions
{
    internal class Leblanc : Plugin
    {
        public enum RSpell
        {
            Q,
            W,
            E,
            Unknown
        }

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static List<Spell> SpellList;
        public static Items.Item Dfg;

        public Leblanc()
        {
            Q = new Spell(SpellSlot.Q, 720);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R);

            // W Delay to be tested ; w ;
            W.SetSkillshot(0, 220, 1500, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 70, 1600, true, SkillshotType.SkillshotLine);

            // Populate spell list
            SpellList = new List<Spell> {Q, R, W, E};
            // Create DFG item
            Dfg = new Items.Item(3128, 750);

            // Setup Events
            Game.OnGameUpdate += GameOnOnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Drawing.OnDraw += Drawing_OnDraw;

            PrintChat("LeBlanc loaded!");
        }

        public RSpell RStatus
        {
            get
            {
                var name = Player.Spellbook.GetSpell(SpellSlot.R).Name;

                switch (name)
                {
                    case "LeblancChaosOrbM":
                        return RSpell.Q;
                    case "LeblancSlideM":
                        return RSpell.W;
                    case "LeblancSoulShackleM":
                        return RSpell.E;
                }

                return RSpell.Unknown;
            }
        }

        public bool WActivated
        {
            get { return Player.Spellbook.GetSpell(SpellSlot.W).Name == "leblancslidereturn"; }
        }

        public bool RActivated
        {
            get { return Player.Spellbook.GetSpell(SpellSlot.R).Name == "leblancslidereturnm"; }
        }

        public bool HasQBuff(Obj_AI_Hero target)
        {
            return target.HasBuff("LeblancChaosOrb") || target.HasBuff("LeblancChaosOrbM");
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            // Use position instead of server position for drawing
            var p = Player.Position;

            foreach (var spell in SpellList.Where(spell => GetBool("draw" + spell.Slot.ToString().ToUpper())))
            {
                Utility.DrawCircle(p, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!GetBool("eInterrupt") || spell.DangerLevel != InterruptableDangerLevel.High)
                return;

            if (E.IsReady())
            {
                E.Cast(unit, Packets);
            }
            else if (R.IsReady() && RStatus == RSpell.E)
            {
                R.Cast(unit, Packets);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!GetBool("eGapcloser"))
                return;

            if (E.IsReady())
            {
                E.Cast(gapcloser.Sender, Packets);
            }
            else if (RStatus == RSpell.E && R.IsReady())
            {
                R.Cast(gapcloser.Sender, Packets);
            }
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
            switch (RStatus)
            {
                case RSpell.Q:
                    R = new Spell(SpellSlot.R, Q.Range);
                    break;
                case RSpell.W:
                    R = new Spell(SpellSlot.R, W.Range);
                    R.SetSkillshot(0.5f, 200, 1200, false, SkillshotType.SkillshotCircle);
                    break;
                case RSpell.E:
                    R = new Spell(SpellSlot.R, E.Range);
                    R.SetSkillshot(0.25f, 100, 1750, true, SkillshotType.SkillshotLine);
                    break;
            }

            switch (OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }
        }

        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                Console.WriteLine("Target is null, returned.");
                return;
            }
                

            if (Dfg.IsReady() && GetBool("useDFG"))
                Dfg.Cast(target);

            // Start combo off with r or q
            if (Q.InRange(target.ServerPosition) && Q.IsReady())
            {
                Q.CastOnUnit(target, Packets);
            }
            else if (RStatus == RSpell.Q && R.InRange(target.ServerPosition) && R.IsReady())
            {
                R.CastOnUnit(target, Packets);
            }

            foreach (var spell in SpellList.Where(x => x.IsReady()).Where(spell => GetBool("use" + spell.Slot)))
            {
                Game.PrintChat("use" + spell.Slot);
                if (spell.Slot == SpellSlot.Q)
                {
                    Q.CastOnUnit(target, Packets);
                }


                if (spell.Slot == SpellSlot.W && !WActivated)
                {
                    W.Cast(target, Packets);
                }

                if (spell.Slot == SpellSlot.E)
                {
                    E.Cast(target, Packets);
                }

                if (spell.Slot != SpellSlot.R)
                {
                    Console.WriteLine("Slot is not R, continued.");
                    continue;
                    
                }

                if (RStatus == RSpell.Q)
                    R.CastOnUnit(target, Packets);

                if (RStatus == RSpell.W && !RActivated)
                    R.Cast(target, Packets);

                else
                    R.Cast(target, Packets);
            }

            if (!GetBool("useWBack") || !target.IsDead) return;
            if (WActivated)
            {
                W.CastOnUnit(Player, Packets);
            }
            else if (RActivated)
            {
                R.CastOnUnit(Player, Packets);
            }
        }

        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            var useQ = GetBool("useQHarass");
            var useW = GetBool("useWHarass");
            var useWBack = GetBool("useWBackHarass");

            if (useQ)
            {
                Q.CastOnUnit(target, Packets);
            }

            if (useW && !WActivated && HasQBuff(target))
            {
                W.Cast(target, Packets);
            }

            if (useWBack && !HasQBuff(target) && WActivated)
                W.CastOnUnit(Player, Packets);
        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;

            if (Q.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);

            if (W.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.W);

            if (E.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.R);

            if (!Dfg.IsReady()) return (float) dmg;
            dmg += Player.GetItemDamage(target, Damage.DamageItems.Dfg);
            dmg += dmg*0.2;

            return (float) dmg;
        }

        public override void Combo(Menu comboMenu)
        {
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useWBack", "W/R back when enemy dead").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
        }

        public override void Harass(Menu harassMenu)
        {
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useWHarass", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("useWBackHarass", "W Back").SetValue(true));
        }

        public override void ItemMenu(Menu itemsMenu)
        {
            itemsMenu.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
        }

        public override void Misc(Menu miscMenu)
        {
            miscMenu.AddItem(new MenuItem("eGapcloser", "E Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("eInterrupt", "E to Interrupt").SetValue(true));
        }

        public override void Drawings(Menu drawingMenu)
        {
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
        }
    }
}