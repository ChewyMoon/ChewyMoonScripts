// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzirOrbwalker.cs" company="ChewyMoon">
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
//   The Orbwalker for Azir. Credits to Kortatu.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Night_Stalker_Azir
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The Orbwalker for Azir. Credits to Kortatu.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
        Justification = "Reviewed. Suppression is OK here.")]
    internal class AzirOrbwalker : Orbwalking.Orbwalker
    {
        #region Constants

        /// <summary>
        ///     The soldier auto attack range
        /// </summary>
        private const int SoldierAutoAttackRange = 250;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AzirOrbwalker" /> class.
        /// </summary>
        /// <param name="attachToMenu">The attach to menu.</param>
        public AzirOrbwalker(Menu attachToMenu)
            : base(attachToMenu)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Checks if a unit is in auto attack range.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>
        ///     <c>1</c> if the player is in range, <c>0</c> if the target cannot be attacked, <c>2</c> if the soldier can
        ///     attack.
        /// </returns>
        public int CustomInAutoAttackRange(AttackableUnit target)
        {
            if (Orbwalking.InAutoAttackRange(target))
            {
                return 1;
            }

            if (!target.IsValidTarget())
            {
                return 0;
            }

            // Azir's soldiers can't attack structures.
            if (!(target is Obj_AI_Base))
            {
                return 0;
            }

            var soldierAArange = SoldierAutoAttackRange + 65 + target.BoundingRadius;
            soldierAArange *= soldierAArange;

            return Program.SandSoldiers.Any(soldier => soldier.Distance(target, true) <= soldierAArange) ? 2 : 0;
        }

        /// <summary>
        ///     Gets the target.
        /// </summary>
        /// <returns>The target.</returns>
        public override AttackableUnit GetTarget()
        {
            if (this.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                || this.ActiveMode == Orbwalking.OrbwalkingMode.Mixed
                || this.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                foreach (var minion in
                    from minion in
                        ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                            minion.IsValidTarget()
                            && minion.Health
                            < 3 * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod))
                    let r = this.CustomInAutoAttackRange(minion)
                    where r != 0
                    let t = (int)(ObjectManager.Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2
                    let predHealth = HealthPrediction.GetHealthPrediction(minion, t, 0)
                    let damage =
                        (r == 1)
                            ? ObjectManager.Player.GetAutoAttackDamage(minion, true)
                            : ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W)
                    where minion.Team != GameObjectTeam.Neutral && MinionManager.IsMinion(minion, true)
                    where predHealth > 0 && predHealth <= damage
                    select minion)
                {
                    return minion;
                }
            }

            if (this.ActiveMode != Orbwalking.OrbwalkingMode.LastHit)
            {
                var posibleTargets = new Dictionary<Obj_AI_Base, float>();
                var autoAttackTarget = TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);

                if (autoAttackTarget.IsValidTarget())
                {
                    posibleTargets.Add(autoAttackTarget, GetDamageValue(autoAttackTarget, false));
                }

                foreach (var soldierTarget in
                    Program.SandSoldiers.Select(
                        soldier =>
                        TargetSelector.GetTarget(
                            SoldierAutoAttackRange + 65 + 65, 
                            TargetSelector.DamageType.Magical, 
                            true, 
                            null, 
                            soldier.ServerPosition)).Where(soldierTarget => soldierTarget.IsValidTarget()))
                {
                    if (posibleTargets.ContainsKey(soldierTarget))
                    {
                        posibleTargets[soldierTarget] *= 1.25f;
                    }
                    else
                    {
                        posibleTargets.Add(soldierTarget, GetDamageValue(soldierTarget, true));
                    }
                }

                if (posibleTargets.Count > 0)
                {
                    return posibleTargets.MinOrDefault(p => p.Value).Key;
                }

                var soldiers = Program.SandSoldiers;
                var objAiBases = soldiers as IList<Obj_AI_Base> ?? soldiers.ToList();

                if (objAiBases.Any())
                {
                    var minions = MinionManager.GetMinions(1100, MinionTypes.All, MinionTeam.NotAlly);
                    var validEnemiesPosition =
                        HeroManager.Enemies.Where(e => e.IsValidTarget(1100))
                            .Select(e => e.ServerPosition.To2D())
                            .ToList();
                    const int AaWidthSqr = 100 * 100;

                    // Try to harass using minions
                    foreach (var soldier in objAiBases)
                    {
                        foreach (var minion in minions)
                        {
                            var soldierAArange = SoldierAutoAttackRange + 65 + minion.BoundingRadius;
                            soldierAArange *= soldierAArange;

                            if (!(soldier.Distance(minion, true) < soldierAArange))
                            {
                                continue;
                            }

                            var p1 = minion.Position.To2D();
                            var p2 = soldier.Position.To2D().Extend(minion.Position.To2D(), 375);
                            if (
                                validEnemiesPosition.Any(
                                    enemyPosition => enemyPosition.Distance(p1, p2, true, true) < AaWidthSqr))
                            {
                                return minion;
                            }
                        }
                    }
                }
            }

            /* turrets / inhibitors / nexus */
            if (this.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                /* turrets */
                foreach (var turret in
                    ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsValidTarget() && Orbwalking.InAutoAttackRange(t)))
                {
                    return turret;
                }

                /* inhibitor */
                foreach (var turret in
                    ObjectManager.Get<Obj_BarracksDampener>()
                        .Where(t => t.IsValidTarget() && Orbwalking.InAutoAttackRange(t)))
                {
                    return turret;
                }

                /* nexus */
                foreach (var nexus in
                    ObjectManager.Get<Obj_HQ>().Where(t => t.IsValidTarget() && Orbwalking.InAutoAttackRange(t)))
                {
                    return nexus;
                }
            }

            /*Jungle minions*/
            if (this.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                || this.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                AttackableUnit result =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            mob =>
                            mob.IsValidTarget() && Orbwalking.InAutoAttackRange(mob)
                            && mob.Team == GameObjectTeam.Neutral)
                        .MaxOrDefault(mob => mob.MaxHealth);
                if (result != null)
                {
                    return result;
                }
            }

            if (this.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                return
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(minion => minion.IsValidTarget() && this.InAutoAttackRange(minion))
                        .MaxOrDefault(m => this.CustomInAutoAttackRange(m) * m.Health);
            }

            return null;
        }

        /// <summary>
        ///     Check if In the automatic attack range.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns><c>true</c> if in auto attack range; else <c>false</c></returns>
        public override bool InAutoAttackRange(AttackableUnit target)
        {
            return this.CustomInAutoAttackRange(target) != 0;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the damage value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="soldierAttack">if set to <c>true</c> [soldier attack].</param>
        /// <returns>The damage.</returns>
        private static float GetDamageValue(Obj_AI_Base target, bool soldierAttack)
        {
            var d = soldierAttack
                        ? ObjectManager.Player.GetSpellDamage(target, SpellSlot.W)
                        : ObjectManager.Player.GetAutoAttackDamage(target);

            return target.Health / (float)d;
        }

        #endregion
    }
}