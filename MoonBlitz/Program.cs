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
//   The program class
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace MoonBlitz
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    /// <summary>
    ///     The program class
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
        ///     Gets or sets the q rectangle.
        /// </summary>
        /// <value>
        ///     The q rectangle.
        /// </value>
        private static Geometry.Polygon.Rectangle QRectangle { get; set; }

        /// <summary>
        ///     Gets or sets the R spell.
        /// </summary>
        /// <value>
        ///     The R spell.
        /// </value>
        private static Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the target.
        /// </summary>
        /// <value>
        ///     The target.
        /// </value>
        private static Obj_AI_Hero Target { get; set; }

        /// <summary>
        ///     Gets or sets the W spell.
        /// </summary>
        /// <value>
        ///     The W spell.
        /// </value>
        private static Spell W { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Called when the program is executed.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Calculates the position to cast a spell according to unit movement.
        /// </summary>
        /// <param name="input">PredictionInput type</param>
        /// <param name="additionalSpeed">Additional Speed (Multiplicative)</param>
        /// <returns>The <see cref="PredictionOutput" /></returns>
        internal static PredictionOutput GetAdvancedPrediction(PredictionInput input, float additionalSpeed = 0)
        {
            var speed = Math.Abs(additionalSpeed) < float.Epsilon ? input.Speed : input.Speed * additionalSpeed;

            if (Math.Abs(speed - int.MaxValue) < float.Epsilon)
            {
                speed = 90000;
            }

            var unit = input.Unit;
            var position = PositionAfter(unit, 1, unit.MoveSpeed - 100);
            var prediction = position
                             + speed
                             * (unit.Direction.To2D().Perpendicular().Normalized() / 2.5f * (input.Delay / 1000))
                             * input.Radius / unit.Distance(Player);

            var hitChance = HitChance.VeryHigh;
            var fixedPred = Player.ServerPosition.Extend(prediction.To3D(), unit.Distance(Player));

            if (Player.Distance(unit) > input.Range)
            {
                hitChance = HitChance.Impossible;
            }

            if (!unit.CanMove)
            {
                hitChance = HitChance.Immobile;
            }

            // Check if by the time the spell arrives the unit will still be in range
            var spellArrivalTime = Player.Distance(fixedPred) / input.Speed * 1000 + input.Delay / 1000;
            var positionAfterArrival = PositionAfter(unit, spellArrivalTime / 1000, unit.MoveSpeed);

            if (Player.Distance(positionAfterArrival) > input.Range)
            {
                hitChance = HitChance.OutOfRange;
            }

            if (!input.Collision)
            {
                return new PredictionOutput()
                           {
                               UnitPosition = new Vector3(position.X, position.Y, unit.ServerPosition.Z), 
                               CastPosition = new Vector3(fixedPred.X, fixedPred.Y, unit.ServerPosition.Z), 
                               Hitchance = hitChance
                           };
            }

            // A circle/cone that has collision...?
            if (input.Collision && input.Type != SkillshotType.SkillshotLine)
            {
                return new PredictionOutput()
                           {
                               UnitPosition = new Vector3(position.X, position.Y, unit.ServerPosition.Z), 
                               CastPosition = new Vector3(fixedPred.X, fixedPred.Y, unit.ServerPosition.Z), 
                               Hitchance = hitChance
                           };
            }

            var positions = new List<Vector3> { position.To3D2(), fixedPred, unit.ServerPosition};
            var collision = LeagueSharp.Common.Collision.GetCollision(positions, input);
            collision.RemoveAll(x => x.NetworkId == input.Unit.NetworkId);

            hitChance = collision.Count > 0 ? HitChance.Collision : hitChance;

            return new PredictionOutput()
                       {
                           UnitPosition = new Vector3(position.X, position.Y, unit.ServerPosition.Z), 
                           CastPosition = new Vector3(fixedPred.X, fixedPred.Y, unit.ServerPosition.Z), 
                           Hitchance = hitChance
                       };
        }

        /// <summary>
        ///     Calculates the unit position after "t"
        /// </summary>
        /// <param name="unit">Unit to track</param>
        /// <param name="t">Track time</param>
        /// <param name="speed">Speed of unit</param>
        /// <returns>The <see cref="Vector2" /> of the after position</returns>
        internal static Vector2 PositionAfter(Obj_AI_Base unit, float t, float speed = float.MaxValue)
        {
            var distance = t * speed;
            var path = unit.GetWaypoints();

            for (var i = 0; i < path.Count - 1; i++)
            {
                var a = path[i];
                var b = path[i + 1];
                var d = a.Distance(b);

                if (d < distance)
                {
                    distance -= d;
                }
                else
                {
                    return a + distance * (b - a).Normalized();
                }
            }

            return path[path.Count - 1];
        }

        /// <summary>
        ///     Called when there is a gap closer approaching.
        /// </summary>
        /// <param name="gapcloser">The gap closer.</param>
        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget(R.Range))
            {
                return;
            }

            if (R.IsReady() && GetValue<bool>("GapcloserR"))
            {
                R.Cast(gapcloser.Sender);
            }
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            (Menu = new Menu("MoonBlitz", "MoonBlitz", true)).AddToMainMenu();

            // Orbwalker
            var owMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(owMenu);
            Menu.AddSubMenu(owMenu);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "TS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Combo
            var comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseREnemies", "Enemies to R").SetValue(new Slider(0x2, 0x1, 0x5)));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "Monika's Ass");
            harassMenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            harassMenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("HarassMana", "Mana Required").SetValue(new Slider(0x4B, 0x1)));
            Menu.AddSubMenu(harassMenu);

            // Kill Steal
            var ksMenu = new Menu("Kill Steal", "KS");
            ksMenu.AddItem(new MenuItem("QKS", "Use Q").SetValue(false));
            ksMenu.AddItem(new MenuItem("RKS", "Use R").SetValue(false));
            Menu.AddSubMenu(ksMenu);

            // Misc
            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("InterruptQ", "Q to Interrupt").SetValue(true));
            miscMenu.AddItem(new MenuItem("InterruptR", "R to Interrupt").SetValue(true));
            miscMenu.AddItem(new MenuItem("GapcloserR", "R on Gapcloser").SetValue(false));
            miscMenu.AddItem(new MenuItem("QOnDash", "Q on Dash").SetValue(true));
            miscMenu.AddItem(
                new MenuItem("MinQRange", "Min. Q Range").SetValue(new Slider(0x1F4, 0, (int)(Q.Range / 0x2))));
            miscMenu.AddItem(
                new MenuItem("Prediction", "Prediction").SetValue(new StringList(new[] { "Common", "Custom" })));
            Menu.AddSubMenu(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Drawing");
            drawMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawMinQRange", "Draw Min. Q Range").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawQPred", "Draw Q Prediction").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            Menu.AddSubMenu(drawMenu);
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private static void DoCombo()
        {
            var useQ = GetValue<bool>("UseQCombo");
            var minQRange = GetValue<Slider>("MinQRange").Value;
            var useW = GetValue<bool>("UseWCombo");
            var useR = GetValue<bool>("UseRCombo");
            var useREnemies = GetValue<Slider>("UseREnemies").Value;

            if (!Target.IsValidTarget())
            {
                return;
            }

            if (useW && W.IsReady() && Player.Distance(Target) > Q.Range / 0x3)
            {
                W.Cast();
            }

            if (useQ && Q.IsReady() && Player.Distance(Target) > minQRange)
            {
                var prediction = GetPrediction(Target);

                if (prediction.Hitchance != HitChance.OutOfRange && prediction.Hitchance != HitChance.Collision
                    && prediction.Hitchance != HitChance.Impossible)
                {
                    Q.Cast(prediction.CastPosition);
                }
            }           

            if (useR && R.IsReady() && Player.CountEnemiesInRange(R.Range) >= useREnemies)
            {
                R.Cast();
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private static void DoHarass()
        {
            var useQ = GetValue<bool>("UseQHarass");
            var minQRange = GetValue<Slider>("MinQRange").Value;
            var useW = GetValue<bool>("UseWHarass");
            var harassMana = GetValue<Slider>("HarassMana").Value;

            if (!Target.IsValidTarget() || Player.ManaPercent < harassMana)
            {
                return;
            }

            if (useW && W.IsReady() && Player.Distance(Target) < Q.Range / 0x2)
            {
                W.Cast();
            }

            if (useQ && Q.IsReady() && Player.Distance(Target) > minQRange)
            {
                var prediction = GetPrediction(Target);

                if (prediction.Hitchance != HitChance.OutOfRange && prediction.Hitchance != HitChance.Collision
                    && prediction.Hitchance != HitChance.Impossible)
                {
                    Q.Cast(prediction.CastPosition);
                }
            }         
        }

        /// <summary>
        ///     Called when league draws itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameDraw(EventArgs args)
        {
            var drawQ = GetValue<bool>("DrawQ");
            var drawMinQRange = GetValue<bool>("DrawMinQRange");
            var drawR = GetValue<bool>("DrawR");
            var drawQPred = GetValue<bool>("DrawQPred");

            if (drawQ)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawMinQRange)
            {
                Render.Circle.DrawCircle(Player.Position, GetValue<Slider>("MinQRange").Value, Color.DarkSlateBlue);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawQPred && Q.IsReady() && Target.IsValidTarget(Q.Range))
            {
                QRectangle.Draw(Color.DeepSkyBlue, 3);
            }
        }

        /// <summary>
        ///     Called when the game loads.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Blitzcrank")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 0x41A);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 0x258);

            Q.SetSkillshot(0.25f, 0x46, 0x708, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.335f, 0x258, 347.79999f, false, SkillshotType.SkillshotCircle);

            CreateMenu();

            QRectangle = new Geometry.Polygon.Rectangle(Player.Position, Vector3.Zero, Q.Width);

            Game.OnUpdate += GameUpdate;
            Drawing.OnDraw += GameDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            Obj_AI_Base.OnNewPath += Obj_AI_Base_OnNewPath;
        }

        /// <summary>
        ///     Called when league updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameUpdate(EventArgs args)
        {
            Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Target.IsValidTarget(Q.Range))
            {
                QRectangle.Start = Player.Position.To2D();
                QRectangle.End = GetPrediction(Target).CastPosition.To2D();
                QRectangle.UpdatePolygon();
            }

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && GetValue<bool>("UseECombo"))
                || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && GetValue<bool>("UseEHarass")))
            {
                if (HeroManager.Enemies.Any(x => x.HasBuff("rocketgrab2")))
                {
                    E.Cast();
                }
            }

            KillSteal();

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
        ///     Gets the prediction.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>Cast position.</returns>
        private static PredictionOutput GetPrediction(Obj_AI_Hero target)
        {
            var prediction = GetValue<StringList>("Prediction").SelectedIndex;

            if (prediction == 0)
            {
                return Q.GetPrediction(target);
            }

            return
                GetAdvancedPrediction(
                    new PredictionInput
                        {
                            Unit = target, Speed = Q.Speed, Delay = Q.Delay * 1000, Radius = Q.Width, Range = Q.Range, 
                            Collision = Q.Collision
                        });
        }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <typeparam name="T">Type to get</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>Value from the menu of the type.</returns>
        private static T GetValue<T>(string name)
        {
            return Menu.Item(name).GetValue<T>();
        }

        /// <summary>
        ///     Called when a target is casting an spell that can be interrupted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private static void Interrupter2_OnInterruptableTarget(
            Obj_AI_Hero sender, 
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsValidTarget() || args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            if (R.IsReady() && R.IsInRange(sender) && GetValue<bool>("InterruptR"))
            {
                R.Cast();
            }
            else if (Q.IsReady() && Q.IsInRange(sender) && GetValue<bool>("InterruptQ"))
            {
                Q.Cast(sender);
            }
        }

        /// <summary>
        ///     Checks if enemies around are available to be killed, and steals the kill.
        /// </summary>
        private static void KillSteal()
        {
            var useQ = GetValue<bool>("QKS");
            var useR = GetValue<bool>("RKS");

            if (useQ && Q.IsReady())
            {
                var bestTarget =
                    HeroManager.Enemies.Where(x => x.Distance(Player) < Q.Range && Q.GetDamage(x) > x.Health)
                        .OrderBy(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    Q.Cast(bestTarget);
                }
            }

            if (!useR || !R.IsReady())
            {
                return;
            }

            if (HeroManager.Enemies.Any(x => x.Distance(Player) < R.Range && R.GetDamage(x) > x.Health))
            {
                R.Cast();
            }
        }

        /// <summary>
        ///     Called when a unit registers a new path.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectNewPathEventArgs" /> instance containing the event data.</param>
        private static void Obj_AI_Base_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (GetValue<bool>("QOnDash") && args.IsDash && sender.IsValid<Obj_AI_Hero>()
                && sender.IsValidTarget(Q.Range))
            {
                Q.Cast(sender);
            }
        }

        /// <summary>
        ///     Called before the orbwalker sends an attack command.
        /// </summary>
        /// <param name="args">The <see cref="Orbwalking.BeforeAttackEventArgs" /> instance containing the event data.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!args.Unit.IsMe || !(args.Target is Obj_AI_Hero))
            {
                return;
            }

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && GetValue<bool>("UseECombo"))
                || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && GetValue<bool>("UseEHarass")))
            {
                E.Cast();
            }
        }

        #endregion
    }
}