#region

using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Linq;

#endregion

namespace ChewyMoonsLux
{
    internal class LuxCombo
    {
        private static GameObject _eGameObject;

        public static void OnGameUpdate(EventArgs args)
        {
            ChewyMoonsLux.PacketCast = ChewyMoonsLux.Menu.Item("packetCast").GetValue<bool>();

            if (ChewyMoonsLux.Menu.Item("ultKS").GetValue<bool>())
            {
                KillSecure();
            }

            // should be reversed but w/e
            if (Orbwalking.OrbwalkingMode.Combo == ChewyMoonsLux.Orbwalker.ActiveMode)
            {
                Combo();
            }

            if (ChewyMoonsLux.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }

            if (ChewyMoonsLux.Menu.Item("autoShield").GetValue<KeyBind>().Active)
            {
                AutoShield();
            }
        }

        private static void AutoShield()
        {
            // linq op babbyyy
            foreach (
                var teamMate in
                    from teamMate in
                        ObjectManager.Get<Obj_AI_Base>().Where(teamMate => teamMate.IsAlly && teamMate.IsValid)
                    let hasToBePercent = ChewyMoonsLux.Menu.Item("autoShieldPercent").GetValue<int>()
                    let ourPercent = teamMate.Health / teamMate.MaxHealth * 100
                    where ourPercent <= hasToBePercent && ChewyMoonsLux.W.IsReady()
                    select teamMate)
            {
                ChewyMoonsLux.W.Cast(teamMate, ChewyMoonsLux.PacketCast);
            }
        }

        private static void KillSecure()
        {
            // KILL SECURE MY ASS LOOL
            foreach (
                var hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget())
                        .Where(
                            hero =>
                                ObjectManager.Player.Distance(hero) <= ChewyMoonsLux.R.Range &&
                                ObjectManager.Player.GetSpellDamage(hero, SpellSlot.Q) >= hero.Health &&
                                ChewyMoonsLux.R.IsReady()))
            {
                ChewyMoonsLux.R.Cast(hero, ChewyMoonsLux.PacketCast);
            }
        }

        private static void Harass()
        {
            var useQ = ChewyMoonsLux.Menu.Item("useQHarass").GetValue<bool>();
            var useE = ChewyMoonsLux.Menu.Item("useEHarass").GetValue<bool>();

            var target = SimpleTs.GetTarget(ChewyMoonsLux.Q.Range, SimpleTs.DamageType.Magical);
            if (!target.IsValidTarget()) return;

            if (HasPassive(target)) return;

            if (useQ & ChewyMoonsLux.Q.IsReady() && !HasPassive(target))
            {
                SpellCombo.CastQ(target);
            }

            if (!useE || !ChewyMoonsLux.E.IsReady() || HasPassive(target) || _eGameObject != null) return;
            ChewyMoonsLux.E.Cast(target, ChewyMoonsLux.PacketCast);
        }

        private static void Combo()
        {
            var useQ = ChewyMoonsLux.Menu.Item("useQ").GetValue<bool>();
            var useW = ChewyMoonsLux.Menu.Item("useW").GetValue<bool>();
            var useE = ChewyMoonsLux.Menu.Item("useE").GetValue<bool>();
            var useR = ChewyMoonsLux.Menu.Item("useR").GetValue<bool>();

            var target = SimpleTs.GetTarget(ChewyMoonsLux.Q.Range, SimpleTs.DamageType.Magical);

            var useDfg = ChewyMoonsLux.Menu.Item("useDFG").GetValue<bool>();

            // Pop e
            if (_eGameObject != null)
            {
                var targetsInE = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(_eGameObject.BoundingRadius)).ToList();
                if (targetsInE.Any(leTarget => !HasPassive(leTarget)))
                    ChewyMoonsLux.E.Cast(ChewyMoonsLux.PacketCast);
            }

            if (!target.IsValidTarget()) return;
            if (HasPassive(target)) return;

            if (useDfg)
            {
                if (Items.CanUseItem(3128) && Items.HasItem(3128)) Items.UseItem(3128, target);
            }

            if (ChewyMoonsLux.Q.IsReady() && useQ && !HasPassive(target))
            {
                SpellCombo.CastQ(target);
            }

            if (ChewyMoonsLux.E.IsReady() && useE && !HasPassive(target) && _eGameObject == null)
            {
                ChewyMoonsLux.E.Cast(target, ChewyMoonsLux.PacketCast);
            }

            if (ChewyMoonsLux.W.IsReady() && useW)
            {
                ChewyMoonsLux.W.Cast(Game.CursorPos, ChewyMoonsLux.PacketCast);
            }

            if (target.IsDead) return;
            if (!ChewyMoonsLux.R.IsReady() || !useR || HasPassive(target)) return;

            if (ChewyMoonsLux.Menu.Item("onlyRIfKill").GetValue<bool>())
            {
                if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) >= target.Health)
                {
                    ChewyMoonsLux.R.Cast(target, ChewyMoonsLux.PacketCast);
                }
            }
            else
            {
                ChewyMoonsLux.R.Cast(target, ChewyMoonsLux.PacketCast);
            }
        }

        private static bool HasPassive(Obj_AI_Base target)
        {
            return target.HasBuff("luxilluminatingfraulein");
        }

        public static void OnGameObjectCreate(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("LuxLightstrike_tar_green")) return;
            _eGameObject = sender;
        }

        public static void OnGameObjectDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("LuxLightstrike_tar_green")) return;
            _eGameObject = null; //rip e Kappa
        }
    }
}