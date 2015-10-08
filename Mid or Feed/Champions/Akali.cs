#region



#endregion

namespace Mid_or_Feed.Champions
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ItemData = LeagueSharp.Common.Data.ItemData;

    /// <summary>
    ///     A plugin for Akali.
    /// </summary>
    internal class Akali : Plugin
    {
        #region Fields

        /// <summary>
        ///     The cutlass
        /// </summary>
        private readonly Items.Item cutlass;

        /// <summary>
        ///     The gunblade
        /// </summary>
        private readonly Items.Item gunblade;

        /// <summary>
        ///     The spell list
        /// </summary>
        private readonly List<Spell> spellList;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Akali" /> class.
        /// </summary>
        public Akali()
        {
            this.spellList = new List<Spell>
                                 {
                                     new Spell(SpellSlot.R, 800), new Spell(SpellSlot.Q, 600), new Spell(SpellSlot.E, 325)
                                 };

            this.gunblade = ItemData.Hextech_Gunblade.GetItem();
            this.cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            Game.OnUpdate += this.GameOnOnGameUpdate;
            Drawing.OnDraw += this.DrawingOnOnDraw;

            PrintChat("Akali loaded!");
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Comboes the specified combo menu.
        /// </summary>
        /// <param name="comboMenu">The combo menu.</param>
        public override void Combo(Menu comboMenu)
        {
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
        }

        /// <summary>
        ///     Drawingses the specified drawing menu.
        /// </summary>
        /// <param name="drawingMenu">The drawing menu.</param>
        public override void Drawings(Menu drawingMenu)
        {
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(new Circle(true, Color.White)));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(new Circle(true, Color.White)));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(new Circle(true, Color.White)));
        }

        /// <summary>
        ///     Gets the combo damage.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override float GetComboDamage(Obj_AI_Hero target)
        {
            var damage =
                this.spellList.Where(spell => spell.Level > 0 && spell.IsReady())
                    .Sum(spell => this.Player.GetDamageSpell(target, spell.Slot).CalculatedDamage);

            if (this.gunblade.IsReady())
            {
                damage += this.Player.GetItemDamage(target, Damage.DamageItems.Hexgun);
            }

            if (this.cutlass.IsReady())
            {
                damage += this.Player.GetItemDamage(target, Damage.DamageItems.Bilgewater);
            }

            return (float)damage;
        }

        /// <summary>
        ///     Harasses the specified harass menu.
        /// </summary>
        /// <param name="harassMenu">The harass menu.</param>
        public override void Harass(Menu harassMenu)
        {
            harassMenu.AddItem(new MenuItem("useQHarass", "Use Q").SetValue(true));
        }

        /// <summary>
        ///     Items the menu.
        /// </summary>
        /// <param name="itemsMenu">The items menu.</param>
        public override void ItemMenu(Menu itemsMenu)
        {
            itemsMenu.AddItem(new MenuItem("useGunblade", "Gunblade").SetValue(true));
            itemsMenu.AddItem(new MenuItem("useCutlass", "Use Cutlass").SetValue(true));
        }

        /// <summary>
        ///     Miscs the specified misc menu.
        /// </summary>
        /// <param name="miscMenu">The misc menu.</param>
        public override void Misc(Menu miscMenu)
        {
            miscMenu.AddItem(new MenuItem("gapcloseR", "Gapclose with R").SetValue(true));
            miscMenu.AddItem(new MenuItem("gapcloseAmmo", "^ Charges").SetValue(new StringList(new[] { "2", "3" })));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(
                this.GetSpell(this.spellList, SpellSlot.Q).Range,
                TargetSelector.DamageType.Magical);

            if (target == null && this.GetValue<bool>("gapcloseR"))
            {
                this.GapcloseCombo();
            }

            if (target == null)
            {
                return;
            }

            var useQ = this.GetValue<bool>("useQ");
            var useE = this.GetValue<bool>("useE");
            var useR = this.GetValue<bool>("useR");

            if (this.gunblade.IsReady() && this.GetBool("useGunblade"))
            {
                this.gunblade.Cast(target);
            }

            if (this.cutlass.IsReady() && this.GetBool("useCutlass"))
            {
                this.cutlass.Cast(target);
            }

            foreach (var spell in this.spellList.Where(x => x.IsReady()))
            {
                if (!target.IsValidTarget(spell.Range))
                {
                    return;
                }

                if (spell.Slot == SpellSlot.Q && useQ)
                {
                    spell.CastOnUnit(target, this.Packets);
                }

                if (spell.Slot == SpellSlot.E && useE)
                {
                    spell.Cast(this.Packets);
                }

                if (spell.Slot == SpellSlot.R && useR
                    && this.Player.Distance(target) > Orbwalking.GetRealAutoAttackRange(this.Player))
                {
                    spell.CastOnUnit(target, this.Packets);
                }
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private void DoHarass()
        {
            var q = this.GetSpell(this.spellList, SpellSlot.Q);

            var target = this.Orbwalker.GetTarget() as Obj_AI_Hero;
            if (!target.IsValidTarget() || !q.IsReady() || !this.GetValue<bool>("useQHarass"))
            {
                return;
            }

            q.CastOnUnit(target, this.Packets);
        }

        /// <summary>
        ///     Fired when the game is drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void DrawingOnOnDraw(EventArgs args)
        {
            var q = this.GetValue<Circle>("drawQ");
            var e = this.GetValue<Circle>("drawE");
            var r = this.GetValue<Circle>("drawR");
            var pos = this.Player.Position;

            if (q.Active)
            {
                Render.Circle.DrawCircle(pos, this.GetSpell(this.spellList, SpellSlot.Q).Range, q.Color);
            }

            if (e.Active)
            {
                Render.Circle.DrawCircle(pos, this.GetSpell(this.spellList, SpellSlot.E).Range, e.Color);
            }

            if (r.Active)
            {
                Render.Circle.DrawCircle(pos, this.GetSpell(this.spellList, SpellSlot.R).Range, r.Color);
            }
        }

        /// <summary>
        ///     Fired when teh game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameOnOnGameUpdate(EventArgs args)
        {
            switch (this.OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    this.DoHarass();
                    break;

                case Orbwalking.OrbwalkingMode.Combo:
                    this.DoCombo();
                    break;
            }
        }

        /// <summary>
        ///     Does the gapclose combo.
        /// </summary>
        private void GapcloseCombo()
        {
            var requiredAmmo = this.GetValue<StringList>("gapcloseAmmo").SelectedIndex == 0 ? 2 : 3;
            var ammo = this.Player.Spellbook.GetSpell(SpellSlot.R).Ammo;

            if (!(ammo >= requiredAmmo))
            {
                return;
            }

            var r = this.GetSpell(this.spellList, SpellSlot.R);
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
                MinionManager.GetMinions(this.Player.ServerPosition, r.Range)
                    .Where(x => x.IsValidTarget())
                    .FirstOrDefault(x => x.Distance(target) < r.Range);

            if (minion.IsValidTarget())
            {
                r.CastOnUnit(minion, this.Packets);
            }
        }

        #endregion
    }
}