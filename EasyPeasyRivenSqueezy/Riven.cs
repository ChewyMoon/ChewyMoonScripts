using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace EasyPeasyRivenSqueezy
{
    /// <summary>
    ///     Manages Riven's stuff, such as Passive, Q, Ult, etc., and loads stuff :D
    /// </summary>
    internal class Riven
    {
        //TODO: organize this shit
        public static int QCount
        {
            get
            {
                return Player.HasBuff("RivenTriCleave", true)
                    ? Player.Buffs.FirstOrDefault(x => x.Name == "RivenTriCleave").Count
                    : 0;
            }
        }

        public static Menu Menu { get; set; }
        public static Orbwalking.Orbwalker Orbwalker { get; set; }

        public static float EWRange
        {
            get { return E.Range + W.Range + Player.BoundingRadius; }
        }

        public static Spell Q { get; internal set; }
        public static Spell W { get; internal set; }
        public static Spell E { get; internal set; }
        public static Spell R { get; internal set; }
        public static Spell Ignite { get; internal set; }
        public static int LastQ { get; internal set; }

        public static bool RActivated
        {
            get { return Player.HasBuff("RivenFengShuiEngine", true); }
        }

        public static bool CanWindSlash
        {
            get { return Player.HasBuff("rivenwindslashready", true); }
        }

        public static float PassiveDmg
        {
            get
            {
                double[] dmgMult = { 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5 };
                var dmg = (Player.FlatPhysicalDamageMod + Player.BaseAttackDamage) * dmgMult[Player.Level / 3];
                return (float) dmg;
            }
        }

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static int QDelay { get; set; }

        public static Obj_AI_Base LastTarget { get; set; }

        public static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Riven")
            {
                return; // This guy is missing out..
            }

            CreateMenu();

            Q = new Spell(SpellSlot.Q, 260) { Delay = 0.5f };
            W = new Spell(SpellSlot.W, 260);
            E = new Spell(SpellSlot.E, 250) { Delay = 0.3f, Speed = 1450 };
            R = new Spell(SpellSlot.R, 1100);
            Ignite = new Spell(Player.GetSpellSlot("summonerdot"), 600);

            Q.SetSkillshot(0.5f, 100, 1400, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.25f, 150, 2200, false, SkillshotType.SkillshotCone);

            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Game.OnUpdate += RivenCombo.OnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2OnOnInterruptableTarget;

            Utility.HpBarDamageIndicator.Enabled = true;
            Utility.HpBarDamageIndicator.DamageToUnit = RivenCombo.GetDamage;

            NotificationHandler.ShowWelcome();

            Game.PrintChat("<font color=\"#7CFC00\"><b>EasyPeasyRivenSqueezy:</b></font> Loaded");
        }

        private static void Interrupter2OnOnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsValidTarget(EWRange) || !GetBool("InterruptEW"))
            {
                NotificationHandler.ShowInterrupterAlert(false);
                return;
            }

            E.Cast(sender.Position);
            W.Cast();

            NotificationHandler.ShowInterrupterAlert(true);
        }

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget() || !GetBool("HandleGapclosers"))
            {
                return;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && RivenCombo.CanHardEngage(gapcloser.Sender))
            {
                R.Cast();
                RivenCombo.CastCircleThing();
                W.Cast();
            }
            else if(gapcloser.Sender.IsValidTarget(Q.Range + Player.BoundingRadius) && QCount == 2)
            {
                Q.Cast(gapcloser.Sender.ServerPosition);
            }
            else if (gapcloser.Sender.IsValidTarget(W.Range))
            {
                W.Cast();
            }
            
            NotificationHandler.ShowGapcloserAlert();
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawEngage = Menu.Item("DrawEngageRange").GetValue<bool>();
            var drawQ = Menu.Item("DrawQ").GetValue<bool>();
            var drawW = Menu.Item("DrawW").GetValue<bool>();
            var drawE = Menu.Item("DrawE").GetValue<bool>();
            var drawR = Menu.Item("DrawR").GetValue<bool>();
            var p = Player.Position;

            if (drawEngage)
            {
                Render.Circle.DrawCircle(p, EWRange, E.IsReady() && W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.SData.Name == "RivenFengShuiEngine" && GetBool("KeepRAlive"))
            {
                Utility.DelayAction.Add(
                    (int) (15000 - Game.Ping / 2 - R.Delay * 1000), delegate
                    {
                        if (CanWindSlash)
                        {
                            var bestTarget =
                                ObjectManager.Get<Obj_AI_Hero>()
                                    .Where(x => x.IsValidTarget(R.Range))
                                    .OrderBy(x => x.Health)
                                    .FirstOrDefault();

                            if (bestTarget != null)
                            {
                                R.Cast(bestTarget);
                            }
                        }
                    });
            }
        }

        public static bool CanQ { get; set; }

        public static bool GetBool(string item)
        {
            return Menu.Item(item).GetValue<bool>();
        }

        private static void CreateMenu()
        {
            (Menu = new Menu("EasyPeasyRivenSqueezy", "cmEZPZ_Riven", true)).AddToMainMenu();

            var orbwalkMenu = new Menu("Orbwalker", "orbwalk");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            var tsMenu = new Menu("Target Selector", "ts");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            var comboMenu = new Menu("Combo", "comboerinokurisuishot");
            comboMenu.AddItem(
                new MenuItem("UseROption", "When to Use R").SetValue(
                    new StringList(new[] { "Hard", "Easy", "Probably" })));
            comboMenu.AddItem(new MenuItem("UseRPercent", "Dont R if target health percent less than ").SetValue(new Slider(1, 1)));
            comboMenu.AddItem(
                new MenuItem("UseRIfCantCancel", "Still use R if cannot cancel animation").SetValue(false));
            comboMenu.AddItem(new MenuItem("QExtraDelay", "Extra Q Delay").SetValue(new Slider(0, 0, 1000)));
            comboMenu.AddItem(new MenuItem("DontEIntoWall", "Dont Headbutt Wall With E").SetValue(true));
            comboMenu.AddItem(new MenuItem("DontEInAARange", "Dont Use E if Target is in your AA range").SetValue(true));
            comboMenu.AddItem(new MenuItem("GapcloseQ", "Q Gapclose").SetValue(true));
            comboMenu.AddItem(new MenuItem("GapcloseE", "E Gapclose").SetValue(true));
            comboMenu.AddItem(new MenuItem("FollowTarget", "Follow Target(Magnet)").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            var farmMenu = new Menu("Wave Clear", "cmWC");
            farmMenu.AddItem(new MenuItem("UseFastQ", "Use Fast Q").SetValue(true));
            farmMenu.AddItem(new MenuItem("UseItems", "Use Items").SetValue(false));
            Menu.AddSubMenu(farmMenu);

            var miscMenu = new Menu("Misc", "cmMisc");
            miscMenu.AddItem(new MenuItem("KeepQAlive", "Keep Q Alive").SetValue(true));
            miscMenu.AddItem(new MenuItem("KeepRAlive", "Keep R Alive").SetValue(true));
            miscMenu.AddItem(new MenuItem("HandleGapclosers", "Handle Gapclosers").SetValue(true));
            miscMenu.AddItem(new MenuItem("InterruptEW", "Interrupt with EW").SetValue(true));
            miscMenu.AddItem(new MenuItem("IgniteKillable", "Ignite if Killable").SetValue(true));
            miscMenu.AddItem(new MenuItem("IgniteKS", "Ignite KS").SetValue(true));
            miscMenu.AddItem(new MenuItem("Notifications", "Use Notifications").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            var fleeMenu = new Menu("Flee", "cmFlee");
            fleeMenu.AddItem(new MenuItem("UseQFlee", "Use Q").SetValue(true));
            fleeMenu.AddItem(new MenuItem("UseEFlee", "Use E").SetValue(true));
            fleeMenu.AddItem(new MenuItem("UseGattaGoFast", "Use Ghostblade").SetValue(true));
            fleeMenu.AddItem(new MenuItem("FleeActive", "Flee!").SetValue(new KeyBind(84, KeyBindType.Press)));
            Menu.AddSubMenu(fleeMenu);

            var drawMenu = new Menu("Drawing", "cmDraw");
            drawMenu.AddItem(new MenuItem("DrawEngageRange", "Draw Engage Range").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawW", "Draw W").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            Menu.AddSubMenu(drawMenu);
        }

        private static void Obj_AI_Base_OnPlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Animation.Contains("Spell1"))
            {
                LastQ = Environment.TickCount;

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    // TODO: implement this
                    var movePos =
                        (ObjectManager.Player.Position.To2D() -
                         (Player.BoundingRadius + 10) * ObjectManager.Player.Direction.To2D().Perpendicular()).To3D();
                    
                    Utility.DelayAction.Add(100, () => Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos));
                    Utility.DelayAction.Add((int)(QDelay + 100 + Player.AttackDelay * 100), Orbwalking.ResetAutoAttackTimer);
                }
            }

            if (args.Animation.Contains("Attack") && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear))
            {
                var aaDelay = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                    ? GetBool("UseFastQ") ? Player.AttackDelay * 100  + Game.Ping / 2f : Player.AttackCastDelay * 1000
                    : Player.AttackDelay * 100;
                Utility.DelayAction.Add((int)(QDelay + aaDelay), () =>
                {
                    //Player.IssueOrder(GameObjectOrder.MoveTo, Q.GetPrediction(LastTarget).CastPosition);
                    Q.Cast(LastTarget.Position);
                });
            }
        }
    }
}
