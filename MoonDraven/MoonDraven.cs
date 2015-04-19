using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace MoonDraven
{
    internal class MoonDraven
    {
        internal class QRecticle
        {
            public GameObject Object { get; set; }
            public int ExpireTime { get; set; }

            public Vector3 Position { get { return Object.Position; } }

            public QRecticle(GameObject rectice, int expireTime)
            {
                Object = rectice;
                ExpireTime = expireTime;
            }    
        }

        public Spell E;
        public Spell Q;
        public List<QRecticle> QReticles = new List<QRecticle>();
        public Spell R;
        public Spell W;
        public Orbwalking.Orbwalker Orbwalker { get; set; }
        public Menu Menu { get; set; }

        public Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public int QCount
        {
            get
            {
                return Player.HasBuff("dravenspinningattack")
                    ? Player.Buffs.First(x => x.Name == "dravenspinningattack").Count
                    : 0;
            }
        }

        // Jodus pls
        public float ManaPercent { get { return Player.Mana / Player.MaxMana * 100; } }

        public void Load()
        {
            // Create spells
            Q = new Spell(SpellSlot.Q, Orbwalking.GetRealAutoAttackRange(Player));
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R);

            E.SetSkillshot(0.25f, 130, 1400, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.4f, 160, 2000, true, SkillshotType.SkillshotLine);

            CreateMenu();

            Game.PrintChat("<font color=\"#7CFC00\"><b>MoonDraven:</b></font> Loaded");

            GameObject.OnCreate += GameObjectOnOnCreate;
            GameObject.OnDelete += GameObjectOnOnDelete;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2OnOnInterruptableTarget;
            Drawing.OnDraw += DrawingOnOnDraw;
            Game.OnUpdate += GameOnOnUpdate;
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawE = Menu.Item("DrawE").IsActive();
            var drawAxeLocation = Menu.Item("DrawAxeLocation").IsActive();
            var drawAxeRange = Menu.Item("DrawAxeRange").IsActive();

            if (drawE)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawAxeLocation)
            {
                var bestAxe =
                    QReticles
                        .Where(
                            x =>
                                x.Position.Distance(Game.CursorPos) <
                                Menu.Item("CatchAxeRange").GetValue<Slider>().Value)
                        .OrderBy(x => x.Position.Distance(Game.CursorPos))
                        .FirstOrDefault();

                if (bestAxe != null)
                {
                    Render.Circle.DrawCircle(bestAxe.Position, 120, Color.LimeGreen);
                }

                foreach (var axe in QReticles.Where(x => x.Object.NetworkId != (bestAxe == null ? 0 : bestAxe.Object.NetworkId)))
                {
                    Render.Circle.DrawCircle(axe.Position, 120, Color.Yellow);
                }
            }

            if (drawAxeRange)
            {
                Render.Circle.DrawCircle(Game.CursorPos, Menu.Item("CatchAxeRange").GetValue<Slider>().Value,
                    Color.DodgerBlue);
            }
        }

        private void Interrupter2OnOnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Menu.Item("UseEInterrupt").IsActive() || !E.IsReady() || !sender.IsValidTarget(E.Range))
            {
                return;
            }

            if (args.DangerLevel == Interrupter2.DangerLevel.Medium || args.DangerLevel == Interrupter2.DangerLevel.High)
            {
                E.Cast(sender);
            }
        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("UseEGapcloser").IsActive() || !E.IsReady() || !gapcloser.Sender.IsValidTarget(E.Range))
            {
                return;
            }

            E.Cast(gapcloser.Sender);
        }

        private void GameObjectOnOnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                return;
            }

            QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId);
        }

        private void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                return;
            }

            QReticles.Add(new QRecticle(sender, Environment.TickCount + 1800));
            Utility.DelayAction.Add(1800, () => QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId));
        }

        private void GameOnOnUpdate(EventArgs args)
        {
            var catchOption = Menu.Item("AxeMode").GetValue<StringList>().SelectedIndex;

            if ((catchOption == 0 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) ||
                (catchOption == 1 && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None) || catchOption == 2)
            {
                var bestReticle =
                    QReticles
                        .Where(x => x.Object.Position.Distance(Game.CursorPos) < Menu.Item("CatchAxeRange").GetValue<Slider>().Value)
                        .OrderBy(x => x.Object.Position.Distance(Game.CursorPos))
                        .FirstOrDefault();

                if (bestReticle != null && bestReticle.Object.Position.Distance(Player.ServerPosition) > 110)
                {
                    var eta = 1000* (Player.Distance(bestReticle.Position)/Player.MoveSpeed);
                    var expireTime = bestReticle.ExpireTime - Environment.TickCount;

                    if (eta >= expireTime && Menu.Item("UseWForQ").IsActive())
                    {
                        W.Cast();
                    }

                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, bestReticle.Position);
                    }
                    else
                    {
                        Orbwalker.SetOrbwalkingPoint(bestReticle.Position);
                    }
                }
                else
                {
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                }
            }

            if (W.IsReady() && Menu.Item("UseWSlow").IsActive() && Player.HasBuffOfType(BuffType.Slow))
            {
                W.Cast();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }

            if (Menu.Item("UseHarassToggle").IsActive())
            {
                Harass();
            }
        }

        private void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = Menu.Item("UseQCombo").IsActive();
            var useW = Menu.Item("UseWCombo").IsActive();
            var useE = Menu.Item("UseECombo").IsActive();
            var useR = Menu.Item("UseRCombo").IsActive();

            if (useQ && QCount < 2 && Q.IsReady() && Orbwalker.InAutoAttackRange(target) &&
                !Player.Spellbook.IsAutoAttacking)
            {
                Q.Cast();
            }

            if (useW && W.IsReady() && ManaPercent > Menu.Item("UseWManaPercent").GetValue<Slider>().Value)
            {
                if (Menu.Item("UseWSetting").IsActive())
                {
                    W.Cast();
                }
                else
                {
                    if (!Player.HasBuff("dravenfurybuff"))
                    {
                        W.Cast();
                    }
                }
            }

            if (useE && E.IsReady())
            {
                E.Cast(target);
            }

            if (!useR || !R.IsReady())
            {
                return;
            }

            // Patented Advanced Algorithms D321987
            var killableTarget =
                HeroManager.Enemies.Where(x => x.IsValidTarget(2000))
                    .FirstOrDefault(
                        x => Player.GetSpellDamage(x, SpellSlot.R) * 2 > x.Health && (!Orbwalker.InAutoAttackRange(x) || Player.CountEnemiesInRange(E.Range) > 2));

            if (killableTarget != null)
            {
                R.Cast(killableTarget);
            }
        }

        private void LaneClear()
        {
            var useQ = Menu.Item("UseQWaveClear").IsActive();
            var useW = Menu.Item("UseWWaveClear").IsActive();
            var useE = Menu.Item("UseEWaveClear").IsActive();

            if (ManaPercent < Menu.Item("WaveClearManaPercent").GetValue<Slider>().Value)
            {
                return;
            }

            if (useQ && QCount < 2 && Q.IsReady() && Orbwalker.GetTarget() is Obj_AI_Minion &&
                !Player.Spellbook.IsAutoAttacking)
            {
                Q.Cast();
            }

            if (useW && W.IsReady() && ManaPercent > Menu.Item("UseWManaPercent").GetValue<Slider>().Value)
            {
                if (Menu.Item("UseWSetting").IsActive())
                {
                    W.Cast();
                }
                else
                {
                    if (!Player.HasBuff("dravenfurybuff"))
                    {
                        W.Cast();
                    }
                }
            }

            if (!useE || !E.IsReady())
            {
                return;
            }

            var bestLocation = E.GetLineFarmLocation(MinionManager.GetMinions(E.Range));

            if (bestLocation.MinionsHit > 1)
            {
                E.Cast(bestLocation.Position);
            }
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Menu.Item("UseEHarass").IsActive() && E.IsReady())
            {
                E.Cast(target);
            }
        }

        private void CreateMenu()
        {
            Menu = new Menu("MoonDraven", "cmMoonDraven", true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "ts");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            // Combo
            var comboMenu = new Menu("Combo", "combo");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "harass");
            harassMenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            harassMenu.AddItem(
                new MenuItem("UseHarassToggle", "Harass! (Toggle)").SetValue(new KeyBind(84, KeyBindType.Toggle)));
            Menu.AddSubMenu(harassMenu);

            // Lane Clear
            var laneClearMenu = new Menu("Wave Clear", "waveclear");
            laneClearMenu.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("UseWWaveClear", "Use W").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("UseEWaveClear", "Use E").SetValue(false));
            laneClearMenu.AddItem(new MenuItem("WaveClearManaPercent", "Mana Percent").SetValue(new Slider(50)));
            Menu.AddSubMenu(laneClearMenu);

            // Axe Menu
            var axeMenu = new Menu("Axe Settings", "axeSetting");
            axeMenu.AddItem(
                new MenuItem("AxeMode", "Catch Axe on Mode:").SetValue(new StringList(new[] {"Combo", "Any", "Always"},
                    2)));
            axeMenu.AddItem(new MenuItem("CatchAxeRange", "Catch Axe Range").SetValue(new Slider(800, 120, 1500)));
            axeMenu.AddItem(new MenuItem("UseWForQ", "Use W if Axe too far").SetValue(true));
            Menu.AddSubMenu(axeMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "draw");
            drawMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawAxeLocation", "Draw Axe Location").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawAxeRange", "Draw Axe Catch Range").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            // Misc Menu
            var miscMenu = new Menu("Misc", "misc");
            miscMenu.AddItem(new MenuItem("UseWSetting", "Use W Instantly(When Available)").SetValue(false));
            miscMenu.AddItem(new MenuItem("UseEGapcloser", "Use E on Gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("UseEInterrupt", "Use E to Interrupt").SetValue(true));
            miscMenu.AddItem(new MenuItem("UseWManaPercent", "Use W Mana Percent").SetValue(new Slider(50)));
            miscMenu.AddItem(new MenuItem("UseWSlow", "Use W if Slowed").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            Menu.AddToMainMenu();
        }
    }
}