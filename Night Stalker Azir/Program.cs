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
namespace Night_Stalker_Azir
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The program class.
    /// </summary>
    internal class Program
    {
        #region Public Properties

        /// <summary>
        ///     Gets the sand soldiers.
        /// </summary>
        /// <value>
        ///     The sand soldiers.
        /// </value>
        public static IEnumerable<Obj_AI_Base> SandSoldiers
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(x => x.IsAlly && x.CharData.BaseSkinName.Equals("azirsoldier"));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the azir automatic attack range.
        /// </summary>
        /// <value>
        ///     The azir automatic attack range.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static int AzirSoldierAutoAttackRange
        {
            get
            {
                return 250;
            }
        }

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        private static Spell E { get; set; }

        /// <summary>
        ///     Gets or sets the flash.
        /// </summary>
        /// <value>
        ///     The flash.
        /// </value>
        private static Spell Flash { get; set; }

        /// <summary>
        ///     Gets or sets the last insec notifcation.
        /// </summary>
        /// <value>
        ///     The last insec notifcation.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static int LastInsecNotifcation { get; set; }

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
        private static AzirOrbwalker Orbwalker { get; set; }

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
        ///     Gets or sets the q.
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        private static Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        private static Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the w.
        /// </summary>
        /// <value>
        ///     The w.
        /// </value>
        private static Spell W { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Counters an incoming enemy gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("UseRGapCloser").IsActive())
            {
                return;
            }

            if (R.IsInRange(gapcloser.Sender, R.Range + 150))
            {
                R.Cast(Player.ServerPosition.Extend(gapcloser.Sender.ServerPosition, R.Range));
            }
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("Night Stalker Azir", "NSAzir", true);

            var targetSelectorMenu = new Menu("Target Selector", "TS");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);

            var orbalkerMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new AzirOrbwalker(orbalkerMenu);
            Menu.AddSubMenu(orbalkerMenu);

            var comboMenu = new Menu("Combo Settings", "Combo");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E to Get into AA Range").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRComboFinisher", "Use R if Killable").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("Harass Settings", "Harass");
            harassMenu.AddItem(new MenuItem("UseQHarass", "Use Q if not in Soldier AA Range").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            harassMenu.AddItem(
                new MenuItem("HarassToggle", "Harass! (Toggle)").SetValue(new KeyBind(84, KeyBindType.Toggle)));
            harassMenu.AddItem(new MenuItem("HarassToggleMana", "Harass Mana (Toggle only)").SetValue(new Slider(50)));
            Menu.AddSubMenu(harassMenu);

            var laneClear = new Menu("Lane Clear Settings", "LaneClear");
            laneClear.AddItem(new MenuItem("UseQLaneClear", "Use Q").SetValue(true));
            laneClear.AddItem(new MenuItem("UseWLaneClear", "Use W").SetValue(true));
            laneClear.AddItem(new MenuItem("LaneClearMana", "Lane Clear Mana Percent").SetValue(new Slider(50)));
            Menu.AddSubMenu(laneClear);

            var fleeMenu = new Menu("Flee Settings", "Flee");
            fleeMenu.AddItem(
                new MenuItem("FleeOption", "Flee Mode").SetValue(new StringList(new[] { "E -> Q", "Q -> E" })));
            fleeMenu.AddItem(new MenuItem("Flee", "Flee").SetValue(new KeyBind(90, KeyBindType.Press)));
            Menu.AddSubMenu(fleeMenu);

            var ksMenu = new Menu("Kill Steal Settings", "KS");
            ksMenu.AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseRKS", "Use R").SetValue(false));
            Menu.AddSubMenu(ksMenu);

            var miscMenu = new Menu("Miscellaneous Settings", "Misc");
            miscMenu.AddItem(new MenuItem("UseRInterrupt", "Interrupt with R").SetValue(true));
            miscMenu.AddItem(new MenuItem("UseRGapCloser", "Use R on Gapcloser").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            var insecMenu = new Menu("Insec Settings", "Insec");
            insecMenu.AddItem(
                new MenuItem("InsecActive", "Insec! (Press)").SetValue(new KeyBind(71, KeyBindType.Press)));
            Menu.AddSubMenu(insecMenu);

            var drawingMenu = new Menu("Drawing Settings", "Drawing");
            drawingMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("DrawW", "Draw W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(false));
            drawingMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(false));
            Menu.AddSubMenu(drawingMenu);

            Menu.AddToMainMenu();
        }

        /// <summary>
        ///     Gets the damages to unit.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>The damage.</returns>
        private static float DamageToUnit(Obj_AI_Hero target)
        {
            var damage = 0f;

            if (Q.IsReady())
            {
                damage += Q.GetDamage(target);
            }

            if (Orbwalking.CanAttack())
            {
                damage +=
                    SandSoldiers.Where(x => target.Distance(x) < AzirSoldierAutoAttackRange)
                        .Sum(soldier => W.GetDamage(target));
                damage += (float)Player.GetAutoAttackDamage(target);
            }

            if (R.IsReady())
            {
                damage += R.GetDamage(target);
            }

            return damage;
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private static void DoCombo()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQCombo = Menu.Item("UseQCombo").IsActive();
            var useWCombo = Menu.Item("UseWCombo").IsActive();
            var useECombo = Menu.Item("UseECombo").IsActive();
            var useRComboFinisher = Menu.Item("UseRComboFinisher").IsActive();

            if (W.IsReady() && useWCombo
                && (W.IsInRange(target, W.Range + AzirSoldierAutoAttackRange)
                    || (Q.IsReady() || Q.Instance.State == SpellState.Surpressed)))
            {
                W.Cast(Player.ServerPosition.Extend(target.ServerPosition, W.Range));
            }

            if (Q.IsReady() && useQCombo
                && SandSoldiers.Any(x => HeroManager.Enemies.All(y => y.Distance(x) > AzirSoldierAutoAttackRange)))
            {
                Q.Cast(target);
            }

            if (E.IsReady() && useECombo && !Orbwalking.InAutoAttackRange(Player))
            {
                var soldier =
                    SandSoldiers.FirstOrDefault(
                        x =>
                        x.Distance(target) < Orbwalking.GetRealAutoAttackRange(Player)
                        && x.Distance(target) > AzirSoldierAutoAttackRange);

                if (soldier != null)
                {
                    E.CastOnUnit(soldier);
                }
            }

            if (R.IsReady() && useRComboFinisher && R.GetDamage(target) > target.Health
                && target.Health - R.GetDamage(target) > -100)
            {
                R.Cast(target);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        /// <param name="toggleCall">If the harass was called by the toggle.</param>
        private static void DoHarass(bool toggleCall)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (toggleCall)
            {
                var manaslider = Menu.Item("HarassToggleMana").GetValue<Slider>().Value;

                if (Player.ManaPercent <= manaslider)
                {
                    return;
                }
            }

            var useQHarass = Menu.Item("UseQHarass").IsActive();
            var useWHarass = Menu.Item("UseWHarass").IsActive();

            if (useWHarass && W.IsReady() && W.IsInRange(target, W.Range + AzirSoldierAutoAttackRange))
            {
                W.Cast(Player.ServerPosition.Extend(target.ServerPosition, W.Range));
            }

            if (useQHarass && SandSoldiers.Any(x => x.Distance(target) > AzirSoldierAutoAttackRange))
            {
                Q.Cast(target);
            }
        }

        /// <summary>
        ///     Does the insec.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static void DoInsec()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos.Randomize(-100, 100));

            var target = TargetSelector.GetSelectedTarget();

            if (!target.IsValidTarget())
            {
                if (Environment.TickCount - LastInsecNotifcation >= 5000)
                {
                    Game.PrintChat(
                        "<font color=\"#7CFC00\"><b>Night Stalker Azir:</b></font> Please select a target by left clicking them!");
                    LastInsecNotifcation = Environment.TickCount;
                }

                return;
            }

            if (!Q.IsInRange(target))
            {
                return;
            }

            if (W.IsReady() && E.IsReady() && (Q.IsReady() || Q.Instance.State == SpellState.Surpressed) && R.IsReady()
                && Flash.IsReady())
            {
                W.Cast(Player.ServerPosition.Extend(target.ServerPosition, W.Range));

                Utility.DelayAction.Add(
                    (int)(W.Delay * 1000), 
                    () =>
                        {
                            E.CastOnUnit(SandSoldiers.OrderBy(x => x.Distance(Player)).FirstOrDefault());
                            Q.Cast(Player.ServerPosition.Extend(target.ServerPosition, Q.Range));

                            Utility.DelayAction.Add(
                                (int)(Q.Delay * 1000 + Player.Distance(target) / 2500 * 1000), 
                                () =>
                                    {
                                        Flash.Cast(Player.ServerPosition.Extend(target.ServerPosition, Flash.Range));

                                        var nearestUnit =
                                            ObjectManager.Get<Obj_AI_Base>()
                                                .OrderBy(x => x.Distance(Player))
                                                .FirstOrDefault(
                                                    x => !x.IsMe || !x.CharData.BaseSkinName.Equals("AzirSoldier"));

                                        if (nearestUnit != null)
                                        {
                                            R.Cast(Player.ServerPosition.Extend(nearestUnit.ServerPosition, R.Range));
                                        }
                                    });
                        });
            }
        }

        /// <summary>
        ///     Does the lane clear.
        /// </summary>
        private static void DoLaneClear()
        {
            var manaPercentage = Menu.Item("LaneClearMana").GetValue<Slider>().Value;

            if (Player.ManaPercent <= manaPercentage)
            {
                return;
            }

            var useQLaneClear = Menu.Item("UseQLaneClear").IsActive();
            var useWLaneClear = Menu.Item("UseWLaneClear").IsActive();

            if (useWLaneClear && W.IsReady())
            {
                var position = W.GetCircularFarmLocation(
                    MinionManager.GetMinions(W.Range + AzirSoldierAutoAttackRange), 
                    AzirSoldierAutoAttackRange);

                if (position.MinionsHit > 1)
                {
                    W.Cast(position.Position);
                }
            }

            if (useQLaneClear && (Q.IsReady() || Q.Instance.State == SpellState.Surpressed)
                && SandSoldiers.Any(
                    x =>
                    MinionManager.GetMinions(x.ServerPosition, AzirSoldierAutoAttackRange)
                        .All(y => y.Distance(x) > AzirSoldierAutoAttackRange)))
            {
                var position = Q.GetCircularFarmLocation(
                    MinionManager.GetMinions(Q.Range + AzirSoldierAutoAttackRange), 
                    AzirSoldierAutoAttackRange);

                if (position.MinionsHit > 1)
                {
                    Q.Cast(position.Position);
                }
            }
        }

        /// <summary>
        ///     Called when the game draws itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("DrawQ").IsActive();
            var drawW = Menu.Item("DrawW").IsActive();
            var drawE = Menu.Item("DrawE").IsActive();
            var drawR = Menu.Item("DrawR").IsActive();

            if (drawQ)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        /// <summary>
        ///     Flees this instance.
        /// </summary>
        private static void Flee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos.Randomize(-100, 100));

            if (
                !((Q.IsReady() || Q.Instance.State == SpellState.Surpressed) && W.IsReady()
                  && (E.IsReady() || E.Instance.State == SpellState.Surpressed)))
            {
                return;
            }

            var option = Menu.Item("FleeOption").GetValue<StringList>().SelectedIndex;

            if (option == 0)
            {
                W.Cast(Player.ServerPosition.Extend(Game.CursorPos, W.Range));

                Utility.DelayAction.Add(
                    (int)(W.Delay * 1000), 
                    () =>
                        {
                            E.CastOnUnit(SandSoldiers.OrderBy(x => x.Distance(Player)).FirstOrDefault());
                            Q.Cast(Player.ServerPosition.Extend(Game.CursorPos, Q.Range));
                        });
            }
            else if (option == 1)
            {
                W.Cast(Player.ServerPosition.Extend(Game.CursorPos, W.Range));

                Utility.DelayAction.Add(
                    (int)(W.Delay * 1000), 
                    () =>
                        {
                            Q.Cast(Player.ServerPosition.Extend(Game.CursorPos, Q.Range));
                            E.CastOnUnit(SandSoldiers.OrderBy(x => x.Distance(Player)).FirstOrDefault());
                        });
            }
        }

        /// <summary>
        ///     Called when the game loads.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.CharData.BaseSkinName != "Azir")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 800 + AzirSoldierAutoAttackRange);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 250);
            Flash = new Spell(Player.GetSpellSlot("summonerflash"), 425);

            Q.SetSkillshot(7.5f / 30, 70, 1000, false, SkillshotType.SkillshotLine);
            W.Delay = 0.25f;
            E.SetSkillshot(7.5f / 30, 100, 2000, true, SkillshotType.SkillshotLine);

            CreateMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = DamageToUnit;
            Utility.HpBarDamageIndicator.Enabled = true;

            Game.PrintChat("<font color=\"#7CFC00\"><b>Night Stalker Azir:</b></font> by ChewyMoon & Shiver loaded");

            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        /// <summary>
        ///     The game on update.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass(false);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    DoLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    break;
            }

            if (Menu.Item("HarassToggle").IsActive() && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
            {
                DoHarass(true);
            }

            if (Menu.Item("InsecActive").IsActive())
            {
                DoInsec();
            }

            if (Menu.Item("Flee").IsActive())
            {
                Flee();
            }

            KillSteal();
        }

        /// <summary>
        ///     Interrupters the interruptable target.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static void Interrupter2_OnInterruptableTarget(
            Obj_AI_Hero sender, 
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsValidTarget(R.Range) || args.DangerLevel != Interrupter2.DangerLevel.High
                || args.EndTime - Environment.TickCount < 500 || !Menu.Item("UseRInterrupt").IsActive())
            {
                return;
            }

            R.Cast(sender);
        }

        /// <summary>
        ///     Steals kills.
        /// </summary>
        private static void KillSteal()
        {
            var useQ = Menu.Item("UseQKS").IsActive();
            var useR = Menu.Item("UseRKS").IsActive();

            if (useQ && (Q.IsReady() || Q.Instance.State == SpellState.Surpressed))
            {
                var bestTarget =
                    HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && Q.GetDamage(x) >= x.Health)
                        .OrderByDescending(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    Q.Cast(bestTarget);
                }
            }
            else if (useR && R.IsReady())
            {
                var bestTarget =
                    HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && R.GetDamage(x) >= x.Health)
                        .OrderByDescending(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    R.Cast(bestTarget);
                }
            }
        }

        /// <summary>
        ///     Called when the program starts.
        /// </summary>
        /// <param name="args">The arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion
    }
}