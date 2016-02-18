namespace Luxorious
{
    using System;
    using System.Linq;
    using System.Reflection;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Program
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        private static Spell E { get; set; }

        /// <summary>
        ///     Gets a value indicating whether [e casted].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [e casted]; otherwise, <c>false</c>.
        /// </value>
        private static bool ECasted
        {
            get
            {
                return EObject != null;
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

        private static Obj_AI_Hero Player { get
        {
            return ObjectManager.Player;
        } }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("Someidk", "ChewyLUXFF", true);

            var targetSelectorMenu = new Menu("Target Selector", "TS");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);

            var orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
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
            comboMenu.AddItem(
                new MenuItem("SpellWeaveCombo", "Spell Weave in Combo").SetValue(true)
                    .SetTooltip(
                        "Casts a spell, then auto attacks, and then casts a second spell after proc'ing the passive."));
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
            waveClearMenu.AddItem(new MenuItem("HarassMinMana", "Harass Min Mana").SetValue(new Slider(75)));
            Menu.AddSubMenu(waveClearMenu);

            var ksMenu = new Menu("Kill Steal Settings", "KSSettings");
            ksMenu.AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseEKS", "Use E").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseRKS", "Use R").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            // TODO: SHIELD ALLY MENU (cba) + MISC

            var miscMenu = new Menu("Miscellaneous Settings", "MiscSettings");
            miscMenu.AddItem(new MenuItem("QThroughMinions", "Cast Q through 1 Minion").SetValue(true));
            miscMenu.AddItem(new MenuItem("QGapcloser", "Use Q on a Gapcloser").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            var drawMenu = new Menu("Drawing Settings", "DrawSettings");
            drawMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawW", "Draw W").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            Menu.AddItem(new MenuItem("Seperator1", " "));
            Menu.AddItem(new MenuItem("madeby", "Made by ChewyMoon"));
            Menu.AddItem(new MenuItem("Version", "Version: " + Assembly.GetExecutingAssembly().GetName().Version));
                
            Menu.AddToMainMenu();
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
        }

        /// <summary>
        ///     Does the lane clear.
        /// </summary>
        private static void DoLaneClear()
        {
        }

        private static void CastQ(Obj_AI_Hero target)
        {
            if (Menu.Item("QThroughMinions").IsActive())
            {
                var prediction = Q.GetPrediction(target);
                var objects =
                    prediction.CollisionObjects.OrderBy(x => x.Distance(Player));

                var firstObj = objects.ElementAt(0);
                var secondObj = objects.ElementAt(1);

                if (firstObj == null)
                {
                    return;
                }

                if ((firstObj.Type == GameObjectType.obj_AI_Hero && firstObj.IsValidTarget(Q.Range))
                    || (MinionManager.IsMinion(firstObj as Obj_AI_Minion) && secondObj.Type == GameObjectType.obj_AI_Hero
                        && secondObj.IsValidTarget(Q.Range)))
                {
                    Q.Cast(target);
                }
            }
            else
            {
                Q.Cast(target);
            }        
        }

        private static void CastE(Obj_AI_Hero target)
        {
            if (ECasted)
            {
                if (EObject.Position.CountEnemiesInRange(350) >= 1
                    && ObjectManager.Get<Obj_AI_Hero>()
                           .Count(x => x.IsValidTarget(350, true, EObject.Position) && !x.HasPassive()) >= 1)
                {
                    E.Cast();
                }
            }
            else
            {
                E.Cast(target);
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
        }

        /// <summary>
        ///     Fired when the game is loaded.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            Q.SetSkillshot(0.5f, 80, 1200, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 150, 1200, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 275, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.75f, 190, 3000, false, SkillshotType.SkillshotLine);

            CreateMenu();

            GameObject.OnCreate += delegate(GameObject sender, EventArgs args2)
                {
                    if (sender.Name.Contains("LuxLightstrike_tar_"))
                    {
                        EObject = sender;
                    }
                };

            GameObject.OnDelete += delegate(GameObject sender, EventArgs args2)
                {
                    if (sender.Name.Contains("LuxLightstrike_tar_"))
                    {
                        EObject = null;
                    }
                };

            Game.OnUpdate += Game_OnUpdate;
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
        /// Determines whether this instance has passive.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static bool HasPassive(this Obj_AI_Hero target)
        {
            return target.HasBuff("luxilluminatingfraulein");
        }

        #endregion
    }
}