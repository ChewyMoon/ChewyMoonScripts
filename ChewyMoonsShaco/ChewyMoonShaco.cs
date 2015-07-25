#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

#endregion

namespace ChewyMoonsShaco
{
    internal class ChewyMoonShaco
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList;
        public static Items.Item Tiamat;
        public static Items.Item Hydra;
        public static int cloneAct = 0;
        public static Obj_AI_Hero player = ObjectManager.Player;
        public static void OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != "Shaco")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 425);
            E = new Spell(SpellSlot.E, 625);
            R = new Spell(SpellSlot.R, 200);

            SpellList = new List<Spell> { Q, E, W, R };

            CreateMenu();
            Illuminati.Init();

            Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();

            Game.OnUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += OrbwalkingOnAfterAttack;



            Game.PrintChat(
                "<font color=\"#6699ff\"><b>ChewyMoon's Shaco:</b></font> <font color=\"#FFFFFF\">" + "loaded!" +
                "</font>");

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }


        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if(!Menu.Item("Evade").GetValue<bool>())return;
            if (sender.IsAlly) return;
            if (!sender.IsChampion()) return;

            //Need to calc Delay/Time for misille to hit !

            if (DangerDB.TargetedList.Contains(args.SData.Name))
            {
                if (args.Target.IsMe)
                    R.Cast();
            }

            if (DangerDB.CircleSkills.Contains(args.SData.Name))
            {
                if (player.Distance(args.End) < args.SData.LineWidth)
                    R.Cast();
            }

            if (DangerDB.Skillshots.Contains(args.SData.Name))
            {
                if (new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.LineWidth).IsInside(player))
                {
                    R.Cast();
                }
            }
        }


        private static void OrbwalkingOnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            if (!(target is Obj_AI_Hero))
            {
                return;
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Hydra.IsReady())
            {
                Hydra.Cast();
            }
            else if (Tiamat.IsReady())
            {
                Tiamat.Cast();
            }
        }

        private static void CreateMenu()
        {
            (Menu = new Menu("[Chewy's Shaco]", "cmShaco", true)).AddToMainMenu();

            // Target Selector
            var tsMenu = new Menu("Target Selector", "cmShacoTS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalking
            var orbwalkingMenu = new Menu("Orbwalking", "cmShacoOrbwalkin");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkingMenu);
            Menu.AddSubMenu(orbwalkingMenu);

            // Combo
            var comboMenu = new Menu("Combo", "cmShacoCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("cloneOrb", "Clone Orbwalking").SetValue(true));

            comboMenu.AddItem(new MenuItem("useItems", "Use items").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "cmShacoHarass");
            harassMenu.AddItem(new MenuItem("useEHarass", "Use E").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // Ks
            var ksMenu = new Menu("KS", "cmShacoKS");
            ksMenu.AddItem(new MenuItem("ksE", "Use E").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            //Escape
            var escapeMenu = new Menu("Escape", "esc");
            escapeMenu.AddItem(new MenuItem("Escape", "Escape").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            escapeMenu.AddItem(new MenuItem("EscapeR", "Escape With Ultimate").SetValue(new KeyBind(226, KeyBindType.Press)));
            escapeMenu.AddItem(new MenuItem("Evade", "Evade With Ultimate").SetValue(false));

            Menu.AddSubMenu(escapeMenu);

            // ILLUMINATI
            var illuminatiMenu = new Menu("Illuminati", "cmShacoTriangleIlluminatiSp00ky");
            illuminatiMenu.AddItem(new MenuItem("PlaceBox", "Place Box").SetValue(new KeyBind(73, KeyBindType.Press)));
            illuminatiMenu.AddItem(
                new MenuItem("RepairTriangle", "Repair Triangle & Auto Form Triangle").SetValue(true));
            illuminatiMenu.AddItem(new MenuItem("BoxDistance", "Box Distance").SetValue(new Slider(600, 101, 1200)));

            illuminatiMenu.Item("BoxDistance").ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs args)
                {
                    Illuminati.TriangleLegDistance = args.GetNewValue<Slider>().Value;
                };

            Menu.AddSubMenu(illuminatiMenu);

            // Drawing
            var drawingMenu = new Menu("Drawings", "cmShacoDrawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawQPos", "Draw Q Pos").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            Menu.AddSubMenu(drawingMenu);

            // Misc
            var miscMenu = new Menu("Misc", "cmShacoMisc");
            miscMenu.AddItem(new MenuItem("usePackets", "Use packets").SetValue(true));
            miscMenu.AddItem(new MenuItem("stuff", "Let me know of any"));
            miscMenu.AddItem(new MenuItem("stuff2", "other misc features you want"));
            miscMenu.AddItem(new MenuItem("stuff3", "on the thread or IRC"));
            miscMenu.AddItem(new MenuItem("stuff4", "Modded by XcxooxL"));
            Menu.AddSubMenu(miscMenu);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var qCircle = Menu.Item("drawQ").GetValue<bool>();
            var wCircle = Menu.Item("drawW").GetValue<bool>();
            var eCircle = Menu.Item("drawE").GetValue<bool>();
            var qPosCircle = Menu.Item("drawQPos").GetValue<bool>();

            var pos = player.Position;

            if (qCircle)
            {
                Render.Circle.DrawCircle(pos, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (wCircle)
            {
                Render.Circle.DrawCircle(pos, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (eCircle)
            {
                Render.Circle.DrawCircle(pos, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (!qPosCircle)
            {
                return;
            }

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget()))
            {
                Drawing.DrawLine(
                    Drawing.WorldToScreen(enemy.Position), Drawing.WorldToScreen(ShacoUtil.GetQPos(enemy, false)), 2,
                    Color.Aquamarine);
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            if (Menu.Item("EscapeR").GetValue<KeyBind>().Active)
            {
                if (R.IsReady() && Q.IsReady())
                {
                    R.Cast();
                }
                Escape();
            }

            if (Menu.Item("Escape").GetValue<KeyBind>().Active)
            {
                Escape();
            }


            if (Menu.Item("ksE").GetValue<bool>())
            {
                KillSecure();
            }

            if (Menu.Item("PlaceBox").IsActive())
            {
                Illuminati.PlaceInitialBox();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
        }

        public static void Escape()
        {
            Q.Cast(Game.CursorPos);
            player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            var clone = getClone();

            if (clone != null)
            {

                var pos = Game.CursorPos.Extend(clone.Position, clone.Distance(Game.CursorPos) + 2000);
                R.Cast(pos);

            }

            
        }

        public static Obj_AI_Base getClone()
        {
            Obj_AI_Base Clone = null;
            foreach (var unit in ObjectManager.Get<Obj_AI_Base>().Where(clone => !clone.IsMe && clone.Name == player.Name))
            {
                Clone = unit;
            }

            return Clone;

        }

        private static void KillSecure()
        {
            if (!E.IsReady())
            {
                return;
            }

            foreach (var target in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsEnemy)
                    .Where(x => !x.IsDead)
                    .Where(x => x.Distance(player) <= E.Range)
                    .Where(target => player.GetSpellDamage(target, SpellSlot.E) > target.Health))
            {
                E.CastOnUnit(target, Menu.Item("usePackets").GetValue<bool>());
                return;
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range , TargetSelector.DamageType.Physical);

            var useQ = Menu.Item("useQ").GetValue<bool>();
            var useW = Menu.Item("useW").GetValue<bool>();
            var useE = Menu.Item("useE").GetValue<bool>();
            var packets = Menu.Item("usePackets").GetValue<bool>();

            foreach (var spell in SpellList.Where(x => x.IsReady()))
            {
                if (spell.Slot == SpellSlot.Q && useQ)
                {
                    if (!target.IsValidTarget(Q.Range))
                    {
                        continue;
                    }

                    var pos = ShacoUtil.GetQPos(target, true);
                    Q.Cast(pos, packets);
                }


                if(target!=null)
                if (spell.Slot == SpellSlot.R && target.IsValidTarget() && player.Distance(target) < 400 &&
                    player.HasBuff("Deceive") && Menu.Item("useR").GetValue<bool>())
                {
                    R.Cast();
                }

                if (spell.Slot == SpellSlot.W && useW)
                {
                    //TODO: Make W based on waypoints
                    if (!target.IsValidTarget(W.Range))
                    {
                        continue;
                    }

                    var pos = ShacoUtil.GetQPos(target, true, 100);
                    W.Cast(pos, packets);
                }

                if (spell.Slot != SpellSlot.E || !useE)
                {
                    continue;
                }
                if (!target.IsValidTarget(E.Range))
                {
                    continue;
                }

                E.CastOnUnit(target);
            }

            if (!Menu.Item("cloneOrb").GetValue<bool>()) return;
            if(!hasClone())return;
            Obj_AI_Base clone = getClone();

                if (Environment.TickCount > cloneAct + 200)
                {
                    if (target != null)
                    {
                        if (clone.IsWindingUp)
                            return;
                        R.Cast(target);
                    }
                    else
                    {
                        R.Cast(Game.CursorPos);
                    }
                    cloneAct = Environment.TickCount;
                }
            
        }

        private static void Harass()
        {
            var useE = Menu.Item("useEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget(E.Range))
            {
                return;
            }

            if (useE && E.IsReady())
            {
                E.CastOnUnit(target);
            }
        }

        public static bool hasClone()
        {
            return player.GetSpell(SpellSlot.R).Name.Equals("hallucinateguide");
        }
    }
}