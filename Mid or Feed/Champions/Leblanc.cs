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

            W.SetSkillshot(0.5f, 220, 1500, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 70, 1600, true, SkillshotType.SkillshotLine);

            // Populate spell list
            SpellList = new List<Spell> { Q, W, E, R };

            // Create DFG item
            Dfg = ItemData.Deathfire_Grasp.GetItem();

            // Setup Events
            Game.OnUpdate += GameOnOnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPossibleToInterrupt;
            Drawing.OnDraw += Drawing_OnDraw;
            //Obj_AI_Base.OnIssueOrder += ObjAiBaseOnOnIssueOrder;

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

        public bool HasValidClone
        {
            get
            {
                var clone = Player.Pet as Obj_AI_Base;
                return clone != null && clone.IsValid && !clone.IsDead;
            }
        }

        public bool HasQBuff(Obj_AI_Hero target)
        {
            return target.HasBuff("LeblancChaosOrb", true) || target.HasBuff("LeblancChaosOrbM", true);
        }

        public bool HasEBuff(Obj_AI_Base target)
        {
            return target.HasBuff("LeblancSoulShackle", true) || target.HasBuff("LeblancSoulShackleM", true);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            // Use position instead of server position for drawing
            var p = Player.Position;

            foreach (var spell in
                SpellList.Where(x => x.Slot != SpellSlot.R).Where(x => GetBool(string.Format("draw{0}", x.Slot))))
            {
                Render.Circle.DrawCircle(p, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!GetBool("eInterrupt") || args.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            if (E.IsReady())
            {
                E.Cast(sender, Packets);
            }
            else if (R.IsReady() && RStatus == RSpell.E)
            {
                R.Cast(sender, Packets);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!GetBool("eGapcloser"))
            {
                return;
            }

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
            if (Menu.Item("Flee").GetValue<KeyBind>().Active)
            {
                Flee();
            }

            //Setup prediction for R spell
            switch (RStatus)
            {
                case RSpell.Q:
                    R = new Spell(SpellSlot.R, Q.Range);
                    break;
                case RSpell.W:
                    R = new Spell(SpellSlot.R, W.Range);
                    R.SetSkillshot(W.Delay, W.Width, W.Speed, W.Collision, SkillshotType.SkillshotCircle);
                    break;
                case RSpell.E:
                    R = new Spell(SpellSlot.R, E.Range);
                    R.SetSkillshot(E.Delay, E.Width, E.Speed, E.Collision, SkillshotType.SkillshotLine);
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

            //DoCloneLogic();
        }

        private void Flee()
        {
            var useJump = GetBool("Flee.UseW");
            var useE = GetBool("Flee.UseE");
            var doubleJump = GetBool("Flee.DoubleW");

            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            var closestEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget())
                    .OrderBy(x => x.Distance(Player))
                    .FirstOrDefault();

            var jumpedW = false;
            var jumpedR = false;


            if (useE && E.IsReady() && closestEnemy != null)
            {
                E.Cast(closestEnemy, Packets);
            }

            if (useJump && W.IsReady() && !WActivated)
            {
                W.Cast(Game.CursorPos, Packets);
                jumpedW = true;
            }
            else if (useJump && RStatus == RSpell.W && R.IsReady() && !RActivated)
            {
                R.Cast(Game.CursorPos, Packets);
                jumpedR = true;
            }

            if (doubleJump && (jumpedW || jumpedR))
            {
                if (jumpedW && R.IsReady() && RStatus == RSpell.W && !RActivated)
                {
                    R.Cast(Game.CursorPos, Packets);
                }

                else if (jumpedR && W.IsReady() && !WActivated)
                {
                    W.Cast(Game.CursorPos, Packets);
                }
            }
        }

        private void DoCombo()
        {
            // Prioritize target in Q range to use q
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical) ??
                         TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var dontDoubleE = GetBool("DontDoubleE");

            if (target == null)
            {
                return;
            }

            if (Dfg.IsReady() && GetBool("useDFG"))
            {
                Dfg.Cast(target);
            }

            foreach (var spell in SpellList.Where(x => x.IsReady()).Where(spell => GetBool("use" + spell.Slot)))
            {
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
                    if (dontDoubleE && !HasEBuff(target))
                    {
                        E.Cast(target, Packets);
                    }
                    else
                    {
                        E.Cast(target, Packets);
                    }
                }

                if (spell.Slot == SpellSlot.R)
                {
                    if (RStatus == RSpell.Q)
                    {
                        R.CastOnUnit(target, Packets);
                    }

                    if (RStatus == RSpell.W && !RActivated)
                    {
                        R.Cast(target, Packets);
                    }

                    else if (RStatus == RSpell.E)
                    {
                        if (dontDoubleE && !HasEBuff(target))
                        {
                            R.Cast(target, Packets);
                        }
                        else
                        {
                            R.Cast(target, Packets);
                        }
                    }
                }
            }

            if (!GetBool("useWBack") || !target.IsDead)
            {
                return;
            }
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
            {
                return;
            }

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
            {
                W.Cast();
            }
        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;

            if (Q.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (W.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (R.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.R);
            }

            if (!Dfg.IsReady())
            {
                return (float) dmg;
            }
            dmg += Player.GetItemDamage(target, Damage.DamageItems.Dfg);
            dmg += dmg * 0.2;

            return (float) dmg;
        }

        public override void Combo(Menu comboMenu)
        {
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useWBack", "W/R back when enemy dead").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("DontDoubleE", "Dont Double Chain").SetValue(true));
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
            var fleeMenu = new Menu("Flee", "mofLbFlee");
            fleeMenu.AddItem(new MenuItem("Flee.UseW", "Use W/R").SetValue(true));
            fleeMenu.AddItem(new MenuItem("Flee.DoubleW", "Double Jump(W + R)").SetValue(true));
            fleeMenu.AddItem(new MenuItem("Flee.UseE", "Use E").SetValue(true));
            miscMenu.AddSubMenu(fleeMenu);

            miscMenu.AddItem(new MenuItem("eGapcloser", "E Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("eInterrupt", "E to Interrupt").SetValue(true));
            //miscMenu.AddItem(
            //  new MenuItem("CloneLogic", "Clone Logic").SetValue(
            //  new StringList(new[] {"Follow", "To Target"})));
            // miscMenu.AddItem(new MenuItem("FollowDelay", "Clone Follow Delay(MS)").SetValue(new Slider(300, 0, 1000)));
            miscMenu.AddItem(new MenuItem("Flee", "Flee!").SetValue(new KeyBind('z', KeyBindType.Press)));
        }

        public override void Drawings(Menu drawingMenu)
        {
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
        }

        #region Clone Logic

        private void DoCloneLogic()
        {
            var clone = Player.Pet as Obj_AI_Base;

            // Don't have clone or not valid
            if (!HasValidClone)
            {
                return;
            }

            switch (Menu.Item("CloneLogic").GetValue<StringList>().SelectedValue)
            {
                // Follow
                case "Follow":
                    var delay = Menu.Item("FollowDelay").GetValue<Slider>().Value;
                    var moveTo = Player.GetWaypoints().Count < 1
                        ? Player.ServerPosition
                        : Player.GetWaypoints().FirstOrDefault().To3D();

                    Utility.DelayAction.Add(
                        delay, () =>
                        {
                            if (!HasValidClone)
                            {
                                return;
                            }

                            if (clone != null)
                            {
                                clone.IssueOrder(GameObjectOrder.MovePet, moveTo);
                            }
                        });
                    break;

                // To enemy
                case "To Enemy":
                    var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                    if (clone != null && target != null)
                    {
                        clone.IssueOrder(GameObjectOrder.AutoAttackPet, target);
                    }
                    break;
            }
        }

        // Mirror logic
        private void ObjAiBaseOnOnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Order.ToString().Contains("Pet"))
            {
                return;
            }

            if (Menu.Item("CloneLogic").GetValue<StringList>().SelectedValue != "Mirror Player")
            {
                return;
            }

            if (!HasValidClone)
            {
                return;
            }

            GameObjectOrder convertedOrder;
            switch (args.Order)
            {
                case GameObjectOrder.MoveTo:
                    convertedOrder = GameObjectOrder.MovePet;
                    break;
                case GameObjectOrder.AutoAttack:
                    convertedOrder = GameObjectOrder.AutoAttackPet;
                    break;
                default:
                    convertedOrder = args.Order;
                    break;
            }

            Game.PrintChat("Order: {0} Converted: {1}", args.Order.ToString(), convertedOrder.ToString());

            var clone = Player.Pet as Obj_AI_Base;
            if (clone != null && HasValidClone)
            {
                clone.IssueOrder(convertedOrder, args.Target);
                clone.IssueOrder(convertedOrder, args.TargetPosition);
            }
        }

        #endregion
    }
}