// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ChewyMoon">
//   Copyright (C) 2015 ChewyMoon
//   
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   The program class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ChewyVayne
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using LeagueSharp;
    using LeagueSharp.Common;
    using LeagueSharp.Common.Data;

    using SharpDX;

    using Color = System.Drawing.Color;
    using ItemData = LeagueSharp.Common.Data.ItemData;

    /// <summary>
    ///     The program class.
    /// </summary>
    internal class Program
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the E spell.
        /// </summary>
        /// <value>
        ///     The E spell.
        /// </value>
        private static Spell E { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        private static Menu Menu { get; set; }

        /// <summary>
        ///     Gets or sets the orbwalker.
        /// </summary>
        /// <value>
        ///     The orbwalker.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static Orbwalking.Orbwalker Orbwalker { get; set; }

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        /// <summary>
        ///     Gets or sets the Q spell.
        /// </summary>
        /// <value>
        ///     The Q spell.
        /// </value>
        private static Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the R spell.
        /// </summary>
        /// <value>
        ///     The R Spell.
        /// </value>
        private static Spell R { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Called when there is an incoming gap closer.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget(E.Range) || !GetValue<bool>("GapcloseE"))
            {
                return;
            }

            E.Cast(gapcloser.Sender);
        }

        /// <summary>
        ///     Determines whether this instance can condemn the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="startPos">The start position.</param>
        /// <param name="casting">if set to <c>true</c>, will ward the bush when condemning.</param>
        /// <returns>Whether this instance can condemn the specified target.</returns>
        private static bool CanCondemnStun(Obj_AI_Base target, Vector3 startPos = default(Vector3), bool casting = true)
        {
            if (startPos == default(Vector3))
            {
                startPos = Player.ServerPosition;
            }

            var knockbackPos = startPos.Extend(
                target.ServerPosition, 
                startPos.Distance(target.ServerPosition) + GetValue<Slider>("EDistance").Value);

            var flags = NavMesh.GetCollisionFlags(knockbackPos);
            var collision = flags.HasFlag(CollisionFlags.Building) || flags.HasFlag(CollisionFlags.Wall);

            if (!casting || !GetValue<bool>("Wardbush") || !NavMesh.IsWallOfGrass(knockbackPos, 200))
            {
                return collision;
            }

            var wardItem = Items.GetWardSlot();

            if (!GetValue<bool>("Wardbush"))
            {
                return collision;
            }

            if (wardItem != default(InventorySlot))
            {
                Player.Spellbook.CastSpell(wardItem.SpellSlot, knockbackPos);
            }
            else if (Items.CanUseItem(ItemData.Scrying_Orb_Trinket.Id))
            {
                Items.UseItem(ItemData.Scrying_Orb_Trinket.Id, knockbackPos);
            }
            else if (Items.CanUseItem(ItemData.Farsight_Orb_Trinket.Id))
            {
                Items.UseItem(ItemData.Farsight_Orb_Trinket.Id, knockbackPos);
            }

            return collision;
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            (Menu = new Menu("ChewyVayne XD", "cmVayne", true)).AddToMainMenu();

            // Target Selector
            var tsMenu = new Menu("Target Selector", "TS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            // Combo
            var comboMenu = new Menu("Combo", "C-C-Combo Breaker!");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
            comboMenu.AddItem(new MenuItem("RComboEnemies", "Enemies to use R").SetValue(new Slider(3, 1, 5)));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "Herass XD");
            harassMenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(false));
            Menu.AddSubMenu(harassMenu);

            // Wave Clear
            var waveClearMenu = new Menu("Wave Clear", "waveclearino");
            waveClearMenu.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(false));
            Menu.AddSubMenu(waveClearMenu);

            // Condemn Settings
            var condemnMenu = new Menu("Condemn Settings", "ConDAMM_Settings");
            condemnMenu.AddItem(new MenuItem("EDistance", "E Push Distance").SetValue(new Slider(450, 300, 600)));
            condemnMenu.AddItem(new MenuItem("QIntoE", "Q to E target").SetValue(true));
            condemnMenu.AddItem(new MenuItem("EPeel", "Peel with E").SetValue(true));
            condemnMenu.AddItem(new MenuItem("EKS", "Finish with E").SetValue(true));
            condemnMenu.AddItem(new MenuItem("GapcloseE", "E on Gapcloser").SetValue(true));
            condemnMenu.AddItem(new MenuItem("InterruptE", "E to Interrupt").SetValue(true));
            condemnMenu.AddItem(new MenuItem("Wardbush", "Ward bush on E").SetValue(true));
            Menu.AddSubMenu(condemnMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Drawing");
            drawMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawEPos", "Draw Condemn Location").SetValue(false));
            Menu.AddSubMenu(drawMenu);

            // Version info
            Menu.AddItem(
                new MenuItem(
                    "VersionInformation", 
                    "Version: " + Assembly.GetAssembly(typeof(Program)).GetName().Version));

            // Author
            Menu.AddItem(new MenuItem("Author", "By ChevyMoon"));
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private static void DoCombo()
        {
            var useQ = GetValue<bool>("UseQCombo");
            var useE = GetValue<bool>("UseECombo");
            var useEPeel = GetValue<bool>("EPeel");
            var qIntoE = GetValue<bool>("QIntoE");
            var useR = GetValue<bool>("UseRCombo");
            var useREnemies = GetValue<Slider>("RComboEnemies").Value;
            var useEFinisher = GetValue<bool>("EKS");

            var target = TargetSelector.GetTarget(
                Orbwalking.GetRealAutoAttackRange(Player) + 300, 
                TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (qIntoE && Q.IsReady() && E.IsReady() && !CanCondemnStun(target, default(Vector3), false))
            {
                var predictedPosition = Player.ServerPosition.Extend(Game.CursorPos, Q.Range);

                if (predictedPosition.Distance(target.ServerPosition) < E.Range
                    && CanCondemnStun(target, predictedPosition))
                {
                    Q.Cast(predictedPosition);
                    Utility.DelayAction.Add((int)(Q.Delay * 1000 + Game.Ping / 2f), () => E.Cast(target));
                }
            }

            if (Q.IsReady() && useQ && !Orbwalking.CanAttack()
                && Player.Distance(target) > Orbwalking.GetRealAutoAttackRange(Player))
            {
                Q.Cast(target.Position);
            }

            if (useE && E.IsReady() && CanCondemnStun(target))
            {
                E.Cast(target);
            }

            if (useEPeel && E.IsReady() && !Player.IsFacing(target))
            {
                E.Cast(target);
            }

            if (useR && R.IsReady() && Player.CountEnemiesInRange(1000) >= useREnemies)
            {
                R.Cast();
            }

            if (useEFinisher && E.IsReady() && Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
            {
                E.Cast(target);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private static void DoHarass()
        {
            var useE = GetValue<bool>("UseEHarass");

            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (useE && E.IsReady() && CanCondemnStun(target))
            {
                E.Cast(target);
            }
        }

        /// <summary>
        ///     Called when the game draws itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetValue<bool>("DrawQ");
            var drawE = GetValue<bool>("DrawE");
            var drawEPos = GetValue<bool>("DrawEPos");

            if (drawQ)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawEPos && E.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget()))
                {
                    var knockbackPos = Player.ServerPosition.Extend(
                        enemy.ServerPosition, 
                        Player.Distance(enemy) + GetValue<Slider>("EDistance").Value);

                    var flags = NavMesh.GetCollisionFlags(knockbackPos);

                    Drawing.DrawLine(
                        Drawing.WorldToScreen(enemy.Position), 
                        Drawing.WorldToScreen(knockbackPos), 
                        2, 
                        flags.HasFlag(CollisionFlags.Building) || flags.HasFlag(CollisionFlags.Wall)
                            ? Color.Green
                            : Color.Red);
                }
            }
        }

        /// <summary>
        ///     Gets called when the game has loaded.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Vayne")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 300);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R);

            E.SetTargetted(0.25f, 1200);

            CreateMenu();

            var notification = new Notification("ChewyVayne Loaded", 5000)
                                   {
                                       TextColor = new ColorBGRA(Color.Aqua.R, Color.Aqua.G, Color.Aqua.B, Color.Aqua.A), 
                                       BorderColor = new ColorBGRA(Color.Teal.R, Color.Teal.G, Color.Teal.B, Color.Teal.A), 
                                   };

            Notifications.AddNotification(notification);

            Game.OnUpdate += GameOnOnUpdate;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
            Drawing.OnDraw += DrawingOnOnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2OnOnInterruptableTarget;
        }

        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }
        }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <typeparam name="T">Type of value to get.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>The type of value.</returns>
        private static T GetValue<T>(string name)
        {
            return Menu.Item(name).GetValue<T>();
        }

        /// <summary>
        ///     Called when a unit is casting a spell that can be interrupted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private static void Interrupter2OnOnInterruptableTarget(
            Obj_AI_Hero sender, 
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsValidTarget(E.Range) || args.DangerLevel == Interrupter2.DangerLevel.Low)
            {
                return;
            }

            E.Cast(sender);
        }

        /// <summary>
        ///     Method that gets called when the program starts.
        /// </summary>
        /// <param name="args">The arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        ///     Called when the unit launches an auto attack missile.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="target">The target.</param>
        private static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || !Q.IsReady() || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
            {
                return;
            }

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && !GetValue<bool>("UseQCombo"))
                || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && !GetValue<bool>("UseQHarass"))
                || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && !GetValue<bool>("UseQWaveClear")))
            {
                return;
            }

            var minion = target as Obj_AI_Minion;
            if (minion != null && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (!MinionManager.IsMinion(minion))
                {
                    return;
                }
            }

            if (!(target is Obj_AI_Hero) && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
            {
                return;
            }

            Utility.DelayAction.Add(
                (int)(Player.AttackCastDelay * 1000 + Game.Ping / 2f), 
                () =>
                    {
                        Q.Cast(Game.CursorPos);
                        Orbwalking.ResetAutoAttackTimer();
                        Orbwalker.ForceTarget((Obj_AI_Base)target);
                    });
        }

        #endregion
    }
}