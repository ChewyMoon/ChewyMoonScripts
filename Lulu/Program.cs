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
namespace Lulu_and_Pix
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
    public class Program
    {
        #region Static Fields

        /// <summary>
        ///     The initiators list. Credits to Kortatu
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static readonly Dictionary<string, string> InitiatorsList = new Dictionary<string, string>
                                                                                {
                                                                                    { "aatroxq", "Aatrox" }, 
                                                                                    { "akalishadowdance", "Akali" }, 
                                                                                    { "headbutt", "Alistar" }, 
                                                                                    { "bandagetoss", "Amumu" }, 
                                                                                    { "dianateleport", "Diana" }, 
                                                                                    { "elisespidereinitial", "Elise" }, 
                                                                                    { "crowstorm", "FiddleSticks" }, 
                                                                                    { "fioraq", "Fiora" }, 
                                                                                    { "gragase", "Gragas" }, 
                                                                                    { "hecarimult", "Hecarim" }, 
                                                                                    { "ireliagatotsu", "Irelia" }, 
                                                                                    { "jarvanivdragonstrike", "JarvanIV" }, 
                                                                                    { "jaxleapstrike", "Jax" }, 
                                                                                    { "riftwalk", "Kassadin" }, 
                                                                                    { "katarinae", "Katarina" }, 
                                                                                    { "kennenlightningrush", "Kennen" }, 
                                                                                    { "khazixe", "KhaZix" }, 
                                                                                    { "khazixelong", "KhaZix" }, 
                                                                                    { "blindmonkqtwo", "LeeSin" }, 
                                                                                    { "leonazenithblademissle", "Leona" }, 
                                                                                    { "lissandrae", "Lissandra" }, 
                                                                                    { "ufslash", "Malphite" }, 
                                                                                    { "maokaiunstablegrowth", "Maokai" }, 
                                                                                    { "monkeykingnimbus", "MonkeyKing" }, 
                                                                                    {
                                                                                        "monkeykingspintowin", "MonkeyKing"
                                                                                    }, 
                                                                                    { "summonerflash", "MonkeyKing" }, 
                                                                                    { "nocturneparanoia", "Nocturne" }, 
                                                                                    { "olafragnarok", "Olaf" }, 
                                                                                    { "poppyheroiccharge", "Poppy" }, 
                                                                                    { "renektonsliceanddice", "Renekton" }, 
                                                                                    { "rengarr", "Rengar" }, 
                                                                                    { "reksaieburrowed", "RekSai" }, 
                                                                                    { "sejuaniarcticassault", "Sejuani" }, 
                                                                                    { "shenshadowdash", "Shen" }, 
                                                                                    { "shyvanatransformcast", "Shyvana" }, 
                                                                                    { "shyvanatransformleap", "Shyvana" }, 
                                                                                    { "sionr", "Sion" }, 
                                                                                    { "taloncutthroat", "Talon" }, 
                                                                                    { "threshqleap", "Thresh" }, 
                                                                                    { "slashcast", "Tryndamere" }, 
                                                                                    { "udyrbearstance", "Udyr" }, 
                                                                                    { "urgotswap2", "Urgot" }, 
                                                                                    { "viq", "Vi" }, { "vir", "Vi" }, 
                                                                                    { "volibearq", "Volibear" }, 
                                                                                    { "infiniteduress", "Warwick" }, 
                                                                                    { "yasuorknockupcombow", "Yasuo" }, 
                                                                                    { "zace", "Zac" }
                                                                                };

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
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
        ///     Antis the gapcloser_ on enemy gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget(W.Range) || !Menu.Item("AntiGapcloserW").IsActive())
            {
                return;
            }

            W.CastOnUnit(gapcloser.Sender);
        }

        /// <summary>
        ///     Automatics the r.
        /// </summary>
        private static void AutoR()
        {
            var enemies = Menu.Item("AutoR").GetValue<Slider>().Value;

            var ally =
                HeroManager.Allies.Where(x => R.IsInRange(x) && x.CountEnemiesInRange(210) >= enemies)
                    .OrderByDescending(x => x.CountEnemiesInRange(210))
                    .FirstOrDefault();

            if (ally != null)
            {
                R.CastOnUnit(ally);
            }
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("Lulu", "ChewyShiverLULU", true);

            var tsMenu = new Menu("Target Selector", "TS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            var orbwalkMenu = new Menu("Orbwalker", "Orbwalk");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            var comboMenu = new Menu("Combo Settings", "Combo");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use Smart W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E on Enemy if My HP >").SetValue(new Slider(70)));
            comboMenu.AddItem(new MenuItem("UseEQCombo", "Use EQ").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("Harass Settings", "Harass");
            harassMenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseEQHarass", "Use EQ").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseEHarass", "Use E on Enemy if My HP >").SetValue(new Slider(70)));
            harassMenu.AddItem(
                new MenuItem("HarassToggle", "Harass! (Toggle)").SetValue(new KeyBind(84, KeyBindType.Toggle)));
            harassMenu.AddItem(new MenuItem("HarassMana", "Harass Mana Percent (Toggle Only)").SetValue(new Slider(65)));
            Menu.AddSubMenu(harassMenu);

            var laneClearMenu = new Menu("Lane Clear Settings", "LaneClearFarm");
            laneClearMenu.AddItem(new MenuItem("UseQLaneClear", "Use Q").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("UseELaneClear", "Last Hit with E").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("LaneClearMana", "Lane Clear Mana").SetValue(new Slider(45)));
            Menu.AddSubMenu(laneClearMenu);

            var ultMenu = new Menu("Ultimate Settings", "Ult");
            ultMenu.AddItem(new MenuItem("UltAllies", "Use R on Allies").SetValue(true));
            foreach (var ally in HeroManager.Allies.Select(x => x.ChampionName))
            {
                ultMenu.AddItem(new MenuItem("Blank" + ally, string.Empty));
                ultMenu.AddItem(
                    new MenuItem(string.Format("Ult{0}", ally), string.Format("Use R on {0}", ally)).SetValue(true));
                ultMenu.AddItem(
                    new MenuItem(string.Format("UltHealth{0}", ally), string.Format("{0} Health Percent", ally))
                        .SetValue(new Slider(25)));
            }

            Menu.AddSubMenu(ultMenu);

            var ksMenu = new Menu("KillSteal Settings", "KS");
            ksMenu.AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseEQKS", "Use EQ").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseEKS", "Use E").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            var miscMenu = new Menu("Miscellaneous Settings", "Misc");
            miscMenu.AddItem(new MenuItem("AutoR", "Auto R to Knockup Enemies (AOE)").SetValue(new Slider(3, 1, 5)));
            miscMenu.AddItem(new MenuItem("EngageW", "Use W on Engaging Allies").SetValue(true));
            miscMenu.AddItem(new MenuItem("UseWInterrupt", "Use W to Interrupt").SetValue(true));
            miscMenu.AddItem(new MenuItem("AntiGapcloserW", "Use W on Gapcloser").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            var drawingMenu = new Menu("Drawing", "Drawing");
            drawingMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("DrawW", "Draw W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(false));
            drawingMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            Menu.AddSubMenu(drawingMenu);

            Menu.AddToMainMenu();
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private static void DoCombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var useQ = Menu.Item("UseQCombo").IsActive();
            var useW = Menu.Item("UseWCombo").IsActive();
            var useE = Menu.Item("UseECombo").GetValue<Slider>();
            var useEq = Menu.Item("UseEQCombo").IsActive();

            if (target == null && useEq)
            {
                var targetEq = TargetSelector.GetTarget(Q.Range + E.Range, TargetSelector.DamageType.Magical);

                if (targetEq.IsValidTarget())
                {
                    var eTarget =
                        MinionManager.GetMinions(E.Range)
                            .FirstOrDefault(x => x.IsValidTarget() && x.Distance(targetEq) < Q.Range);

                    if (eTarget == null)
                    {
                        return;
                    }

                    E.Cast(eTarget);

                    Utility.DelayAction.Add(
                        (int)(E.Delay * 1000), 
                        () =>
                            {
                                Q.UpdateSourcePosition(eTarget.ServerPosition, eTarget.ServerPosition);
                                Q.Cast(targetEq);
                                Q.UpdateSourcePosition();
                            });
                }
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.IsReady() && useQ)
            {
                Q.Cast(target);
            }

            if (W.IsReady() && useW && !Player.Spellbook.IsAutoAttacking)
            {
                if (Player.Distance(Game.CursorPos) > target.Distance(Game.CursorPos) && W.IsInRange(target))
                {
                    W.CastOnUnit(target);
                }
                else
                {
                    W.CastOnUnit(Player);
                }
            }

            if (E.IsReady())
            {
                E.CastOnUnit(Player.HealthPercent > useE.Value ? target : Player);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        /// <param name="toggle">if set to <c>true</c> we are harassing.</param>
        private static void DoHarass(bool toggle)
        {
            if (toggle && Player.Mana < Menu.Item("HarassMana").GetValue<Slider>().Value)
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var useQ = Menu.Item("UseQHarass").IsActive();
            var useE = Menu.Item("UseEHarass").GetValue<Slider>();
            var useEq = Menu.Item("UseEQHarass").IsActive();

            if (target == null && useEq)
            {
                var targetEq = TargetSelector.GetTarget(Q.Range + E.Range, TargetSelector.DamageType.Magical);

                if (targetEq.IsValidTarget())
                {
                    var eTarget =
                        MinionManager.GetMinions(E.Range)
                            .FirstOrDefault(x => x.IsValidTarget() && x.Distance(targetEq) < Q.Range);

                    if (eTarget == null)
                    {
                        return;
                    }

                    E.Cast(eTarget);

                    Utility.DelayAction.Add(
                        (int)(E.Delay * 1000), 
                        () =>
                            {
                                Q.UpdateSourcePosition(eTarget.ServerPosition, eTarget.ServerPosition);
                                Q.Cast(targetEq);
                                Q.UpdateSourcePosition();
                            });
                }
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.IsReady() && useQ)
            {
                Q.Cast(target);
            }

            if (E.IsReady())
            {
                E.CastOnUnit(Player.HealthPercent > useE.Value ? target : Player);
            }
        }

        /// <summary>
        ///     Does the lane clear.
        /// </summary>
        private static void DoLaneClear()
        {
            var useQ = Menu.Item("UseQLaneClear").IsActive();
            var useE = Menu.Item("UseELaneClear").IsActive();
            var laneClearMana = Menu.Item("LaneClearMana").GetValue<Slider>().Value;

            if (Player.Mana < laneClearMana)
            {
                return;
            }

            if (useQ && Q.IsReady())
            {
                var location = Q.GetLineFarmLocation(MinionManager.GetMinions(Q.Range));

                if (location.MinionsHit > 2)
                {
                    Q.Cast(location.Position);
                }
            }

            if (!useE || !E.IsReady())
            {
                return;
            }

            var minion =
                MinionManager.GetMinions(E.Range)
                    .FirstOrDefault(
                        x =>
                        x.Health - HealthPrediction.GetHealthPrediction(x, (int)(E.Delay * 1000)) > 0
                        && E.GetDamage(x) > x.Health);

            if (minion != null)
            {
                E.CastOnUnit(minion);
            }
        }

        /// <summary>
        ///     Fired when the game is drawn.
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
        ///     Fired when the game loads.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Lulu")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 925);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 900);

            Q.SetSkillshot(0.25f, 60, 1450, false, SkillshotType.SkillshotLine);
            W.SetTargetted(0.25f, 2000);
            E.SetTargetted(0.25f, float.MaxValue);

            CreateMenu();

            Game.PrintChat("<font color=\"#7CFC00\"><b>Lulu and Pix:</b></font> by ChewyMoon & Shiver loaded");

            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        /// <summary>
        ///     Fired when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass(false);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    DoLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }

            if (Menu.Item("HarassToggle").IsActive() && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
            {
                DoHarass(true);
            }

            if (Menu.Item("UltAllies").IsActive())
            {
                UltAllies();
            }

            AutoR();
            KillSteal();
        }

        /// <summary>
        ///     Interrupts the on interruptable target.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private static void Interrupter2_OnInterruptableTarget(
            Obj_AI_Hero sender, 
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsValidTarget(W.Range) || !Menu.Item("UseWinterrupt").IsActive()
                || args.DangerLevel >= Interrupter2.DangerLevel.Medium)
            {
                return;
            }

            W.CastOnUnit(sender);
        }

        /// <summary>
        ///     Steals the kills.
        /// </summary>
        private static void KillSteal()
        {
            var useQ = Menu.Item("UseQKS").IsActive();
            var useEq = Menu.Item("UseEQKS").IsActive();
            var useE = Menu.Item("UseEKS").IsActive();

            if (useQ && Q.IsReady())
            {
                var qTarget =
                    HeroManager.Enemies.FirstOrDefault(
                        x => x.IsValidTarget() && Q.IsInRange(x) && (Q.GetDamage(x) >= x.Health));

                if (qTarget != null)
                {
                    Q.Cast(qTarget);
                }
            }

            if (useEq && Q.IsReady() && E.IsReady())
            {
                var eqTarget =
                    HeroManager.Enemies.FirstOrDefault(
                        x => x.IsValidTarget() && x.Distance(Player) < Q.Range + E.Range && Q.GetDamage(x) >= x.Health);

                if (eqTarget != null)
                {
                    E.CastOnUnit(eqTarget);

                    Utility.DelayAction.Add(
                        (int)(E.Delay * 1000), 
                        () =>
                            {
                                Q.UpdateSourcePosition(eqTarget.ServerPosition, eqTarget.ServerPosition);
                                Q.Cast(eqTarget);
                                Q.UpdateSourcePosition();
                            });
                }
            }

            if (!useE || !E.IsReady())
            {
                return;
            }

            var eTarget =
                HeroManager.Enemies.FirstOrDefault(
                    x => x.IsValidTarget() && E.IsInRange(x) && E.GetDamage(x) >= x.Health);

            if (eTarget != null)
            {
                E.CastOnUnit(eTarget);
            }
        }

        /// <summary>
        ///     The entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        ///     Called when the game processes a spell cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsAlly || !Menu.Item("EngageW").IsActive())
            {
                return;
            }

            if (InitiatorsList.ContainsKey(args.SData.Name.ToLower()))
            {
                W.Cast(sender);
            }
        }

        /// <summary>
        ///     Uses ultimate on the allies.
        /// </summary>
        private static void UltAllies()
        {
            if (!R.IsReady())
            {
                return;
            }

            var ally =
                HeroManager.Allies.Where(
                    x =>
                    Menu.Item(string.Format("Ult{0}", x.ChampionName)).IsActive()
                    && x.HealthPercent
                    < Menu.Item(string.Format("UltHealth{0}", x.ChampionName)).GetValue<Slider>().Value
                    && x.CountEnemiesInRange(500) != 0).OrderBy(x => x.HealthPercent).FirstOrDefault();

            if (ally != null)
            {
                R.CastOnUnit(ally);
            }
        }

        #endregion
    }
}