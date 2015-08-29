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
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Orianna
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The program.
    /// </summary>
    internal class Program
    {
        #region Static Fields

        /// <summary>
        ///     The initiators list
        /// </summary>
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

        #region Public Properties

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public static Menu Menu { get; set; }

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
        ///     Automatics the r.
        /// </summary>
        private static void AutoR()
        {
            if (!R.IsReady())
            {
                return;
            }

            var enemyMinCount = Menu.Item("AutoREnemies").GetValue<Slider>().Value;

            if (
                HeroManager.Enemies.Count(
                    x => Prediction.GetPrediction(x, R.Delay).UnitPosition.Distance(BallManager.BallPosition) <= R.Range)
                >= enemyMinCount)
            {
                R.Cast(HeroManager.Enemies.FirstOrDefault(x => x.Distance(BallManager.BallPosition) <= R.Range));
            }
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("Orianna", "ChewyShiver Orianna", true);

            var tsMenu = new Menu("Target Selector", "TS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            var orbwalkMenu = new Menu("Orbwalker", "Orbwalk");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            var comboMenu = new Menu("Combo Settings", "Combo");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

            // comboMenu.AddItem(new MenuItem("AutoWEnemies>X", "Auto W Enemies").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("Harass Settings", "Harass");
            harassMenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("Harass", "Harass! (Toggle)").SetValue(new KeyBind(84, KeyBindType.Press)));
            harassMenu.AddItem(new MenuItem("HarassMana", "Harass Mana (toggle only)").SetValue(new Slider(50)));
            Menu.AddSubMenu(harassMenu);

            var laneClearMenu = new Menu("Lane Clear Settings", "LaneClearFarm");
            laneClearMenu.AddItem(new MenuItem("UseLaneClear", "LAne Clear").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("LaneClearMana", "Lane Clear Mana").SetValue(new Slider(45)));
            Menu.AddSubMenu(laneClearMenu);

            var ultMenu = new Menu("Ultimate Settings", "Ult");
            ultMenu.AddItem(new MenuItem("AutoREnemies", "Auto R Enemies").SetValue(new Slider(3, 0, 5)));
            Menu.AddSubMenu(ultMenu);

            var ksMenu = new Menu("KillSteal Settings", "KS");
            ksMenu.AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseWKS", "Use W").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            var miscMenu = new Menu("Miscellaneous Settings", "Misc");
            miscMenu.AddItem(new MenuItem("EngageE", "Use E on Engaging Allies").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            var drawingMenu = new Menu("Drawing", "Drawing");
            drawingMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
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
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = Menu.Item("UseQCombo").IsActive();
            var useW = Menu.Item("UseWCombo").IsActive();
            var useE = Menu.Item("UseECombo").IsActive();
            var useR = Menu.Item("UseRCombo").IsActive();

            if (E.IsReady() && useE)
            {
                if (Q.IsReady() && useQ && !BallManager.BallPosition.Equals(Player.Position))
                {
                    var ally =
                        HeroManager.Allies.Where(x => x.Distance(Player) < E.Range && x.Distance(target) < Q.Range)
                            .OrderBy(x => x.Distance(target))
                            .FirstOrDefault();

                    if (ally != null)
                    {
                        E.CastOnUnit(ally);
                    }
                }

                var oldE = E.Range;
                E.Range = float.MaxValue;
                E.UpdateSourcePosition(BallManager.BallPosition);

                if (E.WillHit(target, ObjectManager.Player.ServerPosition, 0, HitChance.VeryHigh))
                {
                    E.CastOnUnit(Player);
                }

                E.UpdateSourcePosition();
                E.Range = oldE;
            }

            if (Q.IsReady() && useQ)
            {
                Q.Cast(target);
                return;
            }

            if (W.IsReady() && useW)
            {
                W.Cast(target);
                return;
            }

            if (R.IsReady() && useR && R.GetDamage(target) > target.Health)
            {
                R.Cast(target);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        /// <param name="toggle">if set to <c>true</c> checks the toggle mana.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static void DoHarass(bool toggle)
        {
            if (toggle && Player.ManaPercent < Menu.Item("HarassMana").GetValue<Slider>().Value)
            {
                return;
            }

            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = Menu.Item("UseQHarass").IsActive();
            var useW = Menu.Item("UseWHarass").IsActive();
            var useE = Menu.Item("UseEHarass").IsActive();

            if (E.IsReady() && useE)
            {
                var oldE = E.Range;
                E.Range = float.MaxValue;
                E.UpdateSourcePosition(BallManager.BallPosition);

                if (E.WillHit(target, ObjectManager.Player.ServerPosition, 0, HitChance.VeryHigh))
                {
                    E.CastOnUnit(Player);
                }

                E.UpdateSourcePosition();
                E.Range = oldE;
            }

            if (Q.IsReady() && useQ)
            {
                Q.Cast(target);
            }

            if (W.IsReady() && useW)
            {
                W.Cast(target);
            }
        }

        /// <summary>
        ///     Does the lane clear.
        /// </summary>
        private static void DoLaneClear()
        {
            if (Player.ManaPercent <= Menu.Item("LaneClearMana").GetValue<Slider>().Value
                || !Menu.Item("UseLaneClear").IsActive())
            {
                return;
            }

            var location =
                MinionManager.GetBestCircularFarmLocation(
                    MinionManager.GetMinions(Q.Range).Select(x => x.ServerPosition.To2D()).ToList(), 
                    W.Width, 
                    Q.Range);

            if (location.MinionsHit == 0)
            {
                return;
            }

            if (Q.IsReady() && W.IsReady())
            {
                Q.Cast(location.Position);
                Utility.DelayAction.Add(
                    (int)((BallManager.BallPosition.Distance(location.Position.To3D()) / 1.2) - 70 - Game.Ping), 
                    () => W.Cast());
            }
            else if (W.IsReady() && MinionManager.GetMinions(BallManager.BallPosition, W.Width).Count > 0)
            {
                W.Cast();
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
                Render.Circle.DrawCircle(BallManager.BallPosition, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(BallManager.BallPosition, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        /// <summary>
        ///     Fired when the game loads.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Orianna")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 825f);
            W = new Spell(SpellSlot.W, 250f);
            E = new Spell(SpellSlot.E, 1100f);
            R = new Spell(SpellSlot.R, 325f);

            Q.SetSkillshot(0.25f, 80, 1200, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 240f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, 1700f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.6f, 375f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            CreateMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        /// <summary>
        ///     Called when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnUpdate(EventArgs args)
        {
            W.UpdateSourcePosition(BallManager.BallPosition, BallManager.BallPosition);
            R.UpdateSourcePosition(BallManager.BallPosition, BallManager.BallPosition);

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

            if (Menu.Item("Harass").IsActive())
            {
                DoHarass(true);
            }

            AutoR();
            KillSteal();
        }

        /// <summary>
        ///     Kills the steal.
        /// </summary>
        private static void KillSteal()
        {
            var useQKs = Menu.Item("UseQKS").IsActive();
            var useWKs = Menu.Item("UseWKS").IsActive();

            if (useQKs && Q.IsReady())
            {
                var qTarget = HeroManager.Enemies.FirstOrDefault(x => Q.IsInRange(x) && Q.IsKillable(x));

                if (qTarget != null)
                {
                    Q.Cast(qTarget);
                }
            }

            if (useWKs && W.IsReady())
            {
                var wTarget = HeroManager.Enemies.FirstOrDefault(x => W.IsInRange(x) && W.IsKillable(x));

                if (wTarget != null)
                {
                    W.Cast(wTarget);
                }
            }

            if (useQKs && useWKs && Q.IsReady() && W.IsReady())
            {
                var qwTarget =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                        Q.IsInRange(x) && !Q.IsKillable(x) && !W.IsKillable(x)
                        && Q.GetDamage(x) + W.GetDamage(x) >= x.Health);

                if (qwTarget != null)
                {
                    Q.Cast(qwTarget);
                    Utility.DelayAction.Add(
                        (int)(BallManager.BallPosition.Distance(qwTarget.ServerPosition) / 1.2 - 70 - Game.Ping), 
                        () => W.Cast(qwTarget));
                }
            }
        }

        /// <summary>
        ///     The entry point for the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        ///     Fired when the game processes a spell cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy || !Menu.Item("EngageE").IsActive())
            {
                return;
            }

            if (InitiatorsList.ContainsKey(args.SData.Name.ToLower()))
            {
                E.Cast(sender);
            }
        }

        #endregion
    }
}