#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

#endregion

namespace Mid_or_Feed.Champions
{
    internal class Akali : Plugin
    {
        private readonly List<Spell> _spellList;
        private readonly Items.Item Cutlass;
        private readonly Items.Item Gunblade;

        public Akali()
        {
            _spellList = new List<Spell>
            {
                new Spell(SpellSlot.R, 800),
                new Spell(SpellSlot.Q, 600),
                new Spell(SpellSlot.E, 325)
            };

            Gunblade = ItemData.Hextech_Gunblade.GetItem();
            Cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            Game.OnUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;

            PrintChat("Akali loaded!");
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var q = GetValue<Circle>("drawQ");
            var e = GetValue<Circle>("drawE");
            var r = GetValue<Circle>("drawR");
            var pos = Player.Position;

            if (q.Active)
            {
                Render.Circle.DrawCircle(pos, GetSpell(_spellList, SpellSlot.Q).Range, q.Color);
            }

            if (e.Active)
            {
                Render.Circle.DrawCircle(pos, GetSpell(_spellList, SpellSlot.E).Range, e.Color);
            }

            if (r.Active)
            {
                Render.Circle.DrawCircle(pos, GetSpell(_spellList, SpellSlot.R).Range, r.Color);
            }
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
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
            var target = TargetSelector.GetTarget(
                GetSpell(_spellList, SpellSlot.Q).Range, TargetSelector.DamageType.Magical);

            if (target == null && GetValue<bool>("gapcloseR"))
            {
                GapcloseCombo();
            }

            if (target == null)
            {
                return;
            }

            var useQ = GetValue<bool>("useQ");
            var useE = GetValue<bool>("useE");
            var useR = GetValue<bool>("useR");

            if (Gunblade.IsReady() && GetBool("useGunblade"))
            {
                Gunblade.Cast(target);
            }

            if (Cutlass.IsReady() && GetBool("useCutlass"))
            {
                Cutlass.Cast(target);
            }

            foreach (var spell in _spellList.Where(x => x.IsReady()))
            {
                if (!target.IsValidTarget(spell.Range))
                {
                    return;
                }

                if (spell.Slot == SpellSlot.Q && useQ)
                {
                    spell.CastOnUnit(target, Packets);
                }

                if (spell.Slot == SpellSlot.E && useE)
                {
                    spell.Cast(Packets);
                }

                if (spell.Slot == SpellSlot.R && useR &&
                    Player.Distance(target) > Orbwalking.GetRealAutoAttackRange(Player))
                {
                    spell.CastOnUnit(target, Packets);
                }
            }
        }

        private void GapcloseCombo()
        {
            var requiredAmmo = GetValue<StringList>("gapcloseAmmo").SelectedIndex == 0 ? 2 : 3;
            var ammo = Player.Spellbook.GetSpell(SpellSlot.R).Ammo;

            if (!(ammo >= requiredAmmo))
            {
                return;
            }

            var r = GetSpell(_spellList, SpellSlot.R);
            if (!r.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(r.Range * 3, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }


            var minion =
                MinionManager.GetMinions(Player.ServerPosition, r.Range)
                    .Where(x => x.IsValidTarget())
                    .FirstOrDefault(x => x.Distance(target) < r.Range);

            if (minion.IsValidTarget())
            {
                r.CastOnUnit(minion, Packets);
            }
        }

        private void DoHarass()
        {
            var q = GetSpell(_spellList, SpellSlot.Q);

            var target = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (!target.IsValidTarget() || !q.IsReady() || !GetValue<bool>("useQHarass"))
            {
                return;
            }

            q.CastOnUnit(target, Packets);
        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            var damage =
                _spellList.Where(spell => spell.Level > 0 && spell.IsReady())
                    .Sum(spell => Player.GetDamageSpell(target, spell.Slot).CalculatedDamage);

            if (Gunblade.IsReady())
            {
                damage += Player.GetItemDamage(target, Damage.DamageItems.Hexgun);
            }

            if (Cutlass.IsReady())
            {
                damage += Player.GetItemDamage(target, Damage.DamageItems.Bilgewater);
            }

            return (float) damage;
        }

        public override void Combo(Menu comboMenu)
        {
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
        }

        public override void Harass(Menu harassMenu)
        {
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
        }

        public override void ItemMenu(Menu itemsMenu)
        {
            itemsMenu.AddItem(new MenuItem("useGunblade", "Gunblade").SetValue(true));
            itemsMenu.AddItem(new MenuItem("useCutlass", "Use Cutlass").SetValue(true));
        }

        public override void Misc(Menu miscMenu)
        {
            miscMenu.AddItem(new MenuItem("gapcloseR", "Gapclose with R").SetValue(true));
            miscMenu.AddItem(new MenuItem("gapcloseAmmo", "^ Charges").SetValue(new StringList(new[] { "2", "3" })));
        }

        public override void Drawings(Menu drawingMenu)
        {
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(new Circle(true, Color.White)));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(new Circle(true, Color.White)));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(new Circle(true, Color.White)));
        }
    }
}