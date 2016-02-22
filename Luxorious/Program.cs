namespace Luxorious
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    internal class Program
    {
        #region Properties

        /// <summary>
        ///     Gets the automatic attack range.
        /// </summary>
        /// <value>
        ///     The automatic attack range.
        /// </value>
        private static float AutoAttackRange
        {
            get
            {
                return Orbwalking.GetRealAutoAttackRange(Player);
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
        ///     Gets a value indicating whether the E spell was casted.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the E spell was casted; otherwise, <c>false</c>.
        /// </value>
        private static bool ECasted
        {
            get
            {
                return Player.HasBuff("LuxLightStrikeKugel") || EObject != null;
            }
        }

        private static GameObject EObject { get; set; }

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

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget(Q.Range) || !Menu.Item("QGapcloser").IsActive())
            {
                return;
            }

            Q.Cast(gapcloser.Sender);
        }

        private static void AttackableUnit_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            var source = ObjectManager.GetUnitByNetworkId<GameObject>(args.SourceNetworkId);
            var obj = ObjectManager.GetUnitByNetworkId<GameObject>(args.TargetNetworkId);

            if (obj.Type != GameObjectType.obj_AI_Hero || source.Type != GameObjectType.obj_AI_Hero)
            {
                return;
            }

            var hero = (Obj_AI_Hero)obj;

            if (hero.IsEnemy || (!hero.IsMe && !W.IsInRange(obj))
                || !Menu.Item(string.Format("{0}", hero.ChampionName)).IsActive())
            {
                return;
            }

            if (((int)(args.Damage / hero.Health) > Menu.Item("ASDamagePercent").GetValue<Slider>().Value)
                || (hero.HealthPercent < Menu.Item("ASHealthPercent").GetValue<Slider>().Value))
            {
                W.Cast(W.GetPrediction(hero).CastPosition);
            }
        }

        private static void CastE(Obj_AI_Hero target)
        {
            if (Environment.TickCount - E.LastCastAttemptT < E.Delay * 1000)
            {
                return;
            }

            if (ECasted)
            {
                if (EObject.Position.CountEnemiesInRange(350) >= 1
                    && ObjectManager.Get<Obj_AI_Hero>()
                           .Count(x => x.IsValidTarget(350, true, EObject.Position) && !x.HasPassive()) >= 1)
                {
                    E.Cast();
                }
            }
            else if (!target.HasPassive())
            {
                E.Cast(target);
            }
        }

        private static void CastQ(Obj_AI_Hero target)
        {
            if (Menu.Item("QThroughMinions").IsActive())
            {
                var prediction = Q.GetPrediction(target);
                var objects = Q.GetCollision(
                    Player.ServerPosition.To2D(),
                    new List<Vector2>() { prediction.CastPosition.To2D() });

                if (objects.Count == 1 || (objects.Count == 1 && objects.ElementAt(0).IsChampion())
                    || objects.Count <= 1
                    || (objects.Count == 2 && (objects.ElementAt(0).IsChampion() || objects.ElementAt(1).IsChampion())))
                {
                    Q.Cast(prediction.CastPosition);
                }

                else
                {
                    Q.Cast(target);
                }
            }
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("Luxorious", "ChewyLUXFF", true);

            var orbwalkerMenu = new Menu("Orbwalker Settings", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            var comboMenu = new Menu("Combo Settings", "ComboSettings");
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseQSlowedCombo", "Use Q only if Slowed by E").SetValue(false));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(false));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            comboMenu.AddItem(
                new MenuItem("UseRComboMode", "R Mode").SetValue(
                    new StringList(new[] { "Always", "If Killable", "Too far" }, 1)));
            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("Harass Settings", "HarassSettings");
            harassMenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            harassMenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("HarassMinMana", "Harass Min Mana").SetValue(new Slider(50)));
            harassMenu.AddItem(
                new MenuItem("HarassKeybind", "Harass! (toggle)").SetValue(new KeyBind(84, KeyBindType.Toggle)));
            Menu.AddSubMenu(harassMenu);

            var waveClearMenu = new Menu("Waveclear Settings", "WaveClearSettings");
            waveClearMenu.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(false));
            waveClearMenu.AddItem(new MenuItem("UseEWaveClear", "Use E").SetValue(false));
            waveClearMenu.AddItem(new MenuItem("UseRWaveClear", "Use R").SetValue(false));
            waveClearMenu.AddItem(new MenuItem("WaveClearMinMana", "Wave Clear Min Mana").SetValue(new Slider(75)));
            Menu.AddSubMenu(waveClearMenu);

            var ksMenu = new Menu("Kill Steal Settings", "KSSettings");
            ksMenu.AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseEKS", "Use E").SetValue(false));
            ksMenu.AddItem(new MenuItem("UseRKS", "Use R").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            var shieldMenu = new Menu("Auto Shield Settings", "ASSettings");
            var shieldOptionsMenu = new Menu("Options", "ShieldOptions");
            shieldOptionsMenu.AddItem(new MenuItem("ASHealthPercent", "Health Percent").SetValue(new Slider(25)));
            shieldOptionsMenu.AddItem(new MenuItem("ASDamagePercent", "Damage Percent").SetValue(new Slider(20)));
            shieldMenu.AddSubMenu(shieldOptionsMenu);
            HeroManager.Allies.ForEach(
                x =>
                shieldMenu.AddItem(new MenuItem("Shield" + x.ChampionName, "Shield " + x.ChampionName).SetValue(true)));
            Menu.AddSubMenu(shieldMenu);

            var miscMenu = new Menu("Miscellaneous Settings", "MiscSettings");
            miscMenu.AddItem(
                new MenuItem("SpellWeaveCombo", "Spell Weave").SetValue(true)
                    .SetTooltip(
                        "Casts a spell, then auto attacks, and then casts a second spell after proc'ing the passive."));
            miscMenu.AddItem(new MenuItem("QThroughMinions", "Cast Q through minions").SetValue(true));
            miscMenu.AddItem(new MenuItem("QGapcloser", "Use Q on a Gapcloser").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            var drawMenu = new Menu("Drawing Settings", "DrawSettings");
            drawMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawW", "Draw W").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawERad", "Draw E Radius").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            Menu.AddItem(new MenuItem("Seperator1", " "));
            Menu.AddItem(new MenuItem("madeby", "Made by ChewyMoon"));
            Menu.AddItem(new MenuItem("Version", "Version: " + Assembly.GetExecutingAssembly().GetName().Version));

            Menu.AddToMainMenu();

            Menu.Item("HarassKeybind").Permashow();
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private static void DoCombo()
        {
            var useQCombo = Menu.Item("UseQCombo").IsActive();
            var useQSlowedCombo = Menu.Item("UseQSlowedCombo").IsActive();
            var useWCombo = Menu.Item("UseWCombo").IsActive();
            var useECombo = Menu.Item("UseECombo").IsActive();
            var useRCombo = Menu.Item("UseRCombo").IsActive();
            var useRComboMode = Menu.Item("UseRComboMode").GetValue<StringList>().SelectedIndex;
            var spellWeaveCombo = Menu.Item("SpellWeaveCombo").IsActive();

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                if (HeroManager.Enemies.Any(x => R.IsInRange(x)) && useRComboMode == 2 && R.IsReady())
                {
                    R.Cast(target);
                }

                return;
            }

            if (useQCombo && Q.IsReady())
            {
                if (spellWeaveCombo)
                {
                    if (!target.HasPassive())
                    {
                        if (useQSlowedCombo && target.HasBuffOfType(BuffType.Slow))
                        {
                            CastQ(target);
                        }
                        else if (!useQSlowedCombo)
                        {
                            CastQ(target);
                        }
                    }
                }
                else
                {
                    CastQ(target);
                }
            }

            if (useWCombo && W.IsReady())
            {
                W.Cast(Game.CursorPos);
            }

            if (useECombo && E.IsReady())
            {
                CastE(target);
            }

            if (!useRCombo || !R.IsReady())
            {
                return;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (useRComboMode)
            {
                case 0:
                    R.Cast(target);
                    break;
                case 1:
                    if (R.IsKillable(target))
                    {
                        R.Cast(target);
                    }
                    break;
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private static void DoHarass()
        {
            var useQHarass = Menu.Item("UseQHarass").IsActive();
            var useWHarass = Menu.Item("UseWHarass").IsActive();
            var useEHarass = Menu.Item("UseEHarass").IsActive();
            var spellWeaveCombo = Menu.Item("SpellWeaveCombo").IsActive();

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (useQHarass && Q.IsReady())
            {
                if (spellWeaveCombo)
                {
                    if (!target.HasPassive() || target.Distance(Player) > AutoAttackRange)
                    {
                        CastQ(target);
                    }
                }
                else
                {
                    CastQ(target);
                }
            }

            if (useWHarass && W.IsReady())
            {
                W.Cast(Game.CursorPos);
            }

            if (useEHarass && E.IsReady())
            {
                CastE(target);
            }
        }

        /// <summary>
        ///     Does the lane clear.
        /// </summary>
        private static void DoLaneClear()
        {
            var useQWaveClear = Menu.Item("UseQWaveClear").IsActive();
            var useEWaveClear = Menu.Item("UseEWaveClear").IsActive();
            var useRWaveClear = Menu.Item("UseRWaveClear").IsActive();
            var waveClearMana = Menu.Item("WaveClearMinMana").GetValue<Slider>().Value;

            if (Player.ManaPercent < waveClearMana)
            {
                return;
            }

            if (useQWaveClear && Q.IsReady())
            {
                var farmLoc = Q.GetLineFarmLocation(MinionManager.GetMinions(Q.Range));

                if (farmLoc.MinionsHit >= 2)
                {
                    Q.Cast(farmLoc.Position);
                }
            }

            if (useEWaveClear && E.IsReady())
            {
                var farmLoc = E.GetCircularFarmLocation(MinionManager.GetMinions(E.Range));

                if (farmLoc.MinionsHit >= 3)
                {
                    E.Cast(farmLoc.Position);
                }
            }

            if (!useRWaveClear || !R.IsReady())
            {
                return;
            }
            {
                var farmLoc = R.GetLineFarmLocation(MinionManager.GetMinions(R.Range));

                if (farmLoc.MinionsHit >= 10)
                {
                    R.Cast(farmLoc.Position);
                }
            }
        }

        /// <summary>
        ///     Fired when the game is Drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("DrawQ").IsActive();
            var drawW = Menu.Item("DrawW").IsActive();
            var drawE = Menu.Item("DrawE").IsActive();
            var drawErad = Menu.Item("DrawERad").IsActive();

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

            if (drawErad && EObject != null)
            {
                Render.Circle.DrawCircle(EObject.Position, 350, Color.CornflowerBlue);
            }
        }

        /// <summary>
        ///     Fired when the scene has been fully drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Menu.Item("DrawR").IsActive())
            {
                return;
            }

            var pointList = new List<Vector3>();

            for (var i = 0; i < 30; i++)
            {
                var angle = i * Math.PI * 2 / 30;
                pointList.Add(
                    new Vector3(
                        Player.Position.X + R.Range * (float)Math.Cos(angle),
                        Player.Position.Y + R.Range * (float)Math.Sin(angle),
                        Player.Position.Z));
            }

            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                var aonScreen = Drawing.WorldToMinimap(a);
                var bonScreen = Drawing.WorldToMinimap(b);

                Drawing.DrawLine(
                    aonScreen.X,
                    aonScreen.Y,
                    bonScreen.X,
                    bonScreen.Y,
                    1,
                    R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        /// <summary>
        ///     Fired when the game is loaded.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    DoLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                case Orbwalking.OrbwalkingMode.Freeze:
                    break;
                case Orbwalking.OrbwalkingMode.CustomMode:
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Menu.Item("HarassKeybind").IsActive() && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
            {
                DoHarass();
            }

            KillSteal();
            JungleKillSteal();
        }

        /// <summary>
        ///     Fired when the game is loaded.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Lux")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1075);
            R = new Spell(SpellSlot.R, 3000);

            Q.SetSkillshot(0.25f, 70f, 1200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 110f, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.3f, 250f, 1050f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1f, 110f, float.MaxValue, false, SkillshotType.SkillshotLine);

            CreateMenu();

            GameObject.OnCreate += delegate(GameObject sender, EventArgs args2)
                {
                    if (sender.Name.Contains("Lux_Base_E_tar"))
                    {
                        EObject = sender;
                    }
                };

            GameObject.OnDelete += delegate(GameObject sender, EventArgs args2)
                {
                    if (sender.Name.Contains("Lux_Base_E_tar"))
                    {
                        EObject = null;
                    }
                };

            DamageIndicator.DamageToUnit = DamageToUnit;
            DamageIndicator.Enabled = true;

            Game.OnUpdate += Game_OnUpdate;
            AttackableUnit.OnDamage += AttackableUnit_OnDamage;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        private static float DamageToUnit(Obj_AI_Hero hero)
        {
            var damage = 0f;

            if (Q.IsReady())
            {
                damage += Q.GetDamage(hero) + hero.GetPassiveDamage();
            }

            if (E.IsReady())
            {
                damage += E.GetDamage(hero) + hero.GetPassiveDamage();
            }

            if (R.IsReady())
            {
                damage += R.GetDamage(hero) + hero.GetPassiveDamage() * 2;
            }

            return damage;
        }

        /// <summary>
        ///     Last hits jungle mobs with a spell.
        /// </summary>
        private static void JungleKillSteal()
        {
        }

        /// <summary>
        ///     Last hits champions with spells.
        /// </summary>
        private static void KillSteal()
        {
            var spellsToUse =
                new List<Spell>(
                    new[] { Q, E, R }.Where(
                        x => x.IsReady() && Menu.Item("Use" + Enum.GetName(typeof(SpellSlot), x.Slot) + "KS").IsActive()));

            foreach (var enemy in HeroManager.Enemies)
            {
                var spell =
                    spellsToUse.Where(x => x.GetDamage(enemy) > enemy.Health && enemy.IsValidTarget(x.Range))
                        .MinOrDefault(x => x.GetDamage(enemy));

                if (spell == null)
                {
                    continue;
                }

                spell.Cast(enemy);

                return;
            }
        }

        /// <summary>
        ///     The entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        #endregion
    }

    public static class LuxExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Determines whether this instance has passive.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static bool HasPassive(this Obj_AI_Hero target)
        {
            return target.HasBuff("luxilluminatingfraulein");
        }

        public static float GetPassiveDamage(this Obj_AI_Hero target)
        {
            return (float)ObjectManager.Player.CalcDamage(
                target,
                Damage.DamageType.Magical, 
                10 + (8 * ObjectManager.Player.Level) + (0.2 * ObjectManager.Player.TotalMagicalDamage));
        }

        #endregion
    }
}