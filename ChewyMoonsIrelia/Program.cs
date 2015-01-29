#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

#endregion

namespace ChewyMoonsIrelia
{
    internal class Program
    {
        //TODO: Rewrite irelia ult

        private const string ChampName = "Irelia";
        private static Menu _menu;
        private static Spell _q;
        private static Spell _w;
        private static Spell _e;
        private static Spell _r;
        // Irelia Ultimate stuff.
        private static bool _hasToFire;
        private static int _charges;
        private static bool _packetCast;
        public static Items.Item BotRk;
        public static Items.Item Cutlass;
        public static Orbwalking.Orbwalker Orbwalker { get; set; }
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs eventArgs)
            {
                var exception = eventArgs.ExceptionObject as Exception;
                if (exception != null)
                {
                    Console.WriteLine(exception.Message);
                }
            };
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != ChampName)
            {
                return;
            }

            _q = new Spell(SpellSlot.Q, 650);
            _w = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player));
            _e = new Spell(SpellSlot.E, 425);
            _r = new Spell(SpellSlot.R, 1000);

            _r.SetSkillshot(0, 65, 1600f, false, SkillshotType.SkillshotLine);

            SetupMenu();

            BotRk = ItemData.Blade_of_the_Ruined_King.GetItem();
            Cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            // IreliaUpdater.CheckForUpdates();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += InterrupterOnOnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Utility.HpBarDamageIndicator.DamageToUnit += DamageToUnit;
            Utilities.PrintChat("Loaded.");
        }

        private static float DamageToUnit(Obj_AI_Hero hero)
        {
            double dmg = 0;

            if (BotRk.IsReady())
            {
                dmg += ObjectManager.Player.GetItemDamage(hero, Damage.DamageItems.Botrk);
            }

            if (Cutlass.IsReady())
            {
                dmg += ObjectManager.Player.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            }

            if (_q.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.Q);
            }

            if (_w.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.W);
            }

            if (_e.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.E);
            }

            // Damage calc per blade
            if (_r.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R) * 4;
            }

            return (float) dmg;
        }

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!_menu.Item("eOnGapclose").GetValue<bool>())
            {
                return;
            }
            if (!_e.IsReady())
            {
                return;
            }

            _e.Cast(gapcloser.Sender, _packetCast);
        }

        private static void InterrupterOnOnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var spell = args;
            var unit = sender;

            if (!_menu.Item("interruptUlts").GetValue<bool>())
            {
                return;
            }
            if (spell.DangerLevel != Interrupter2.DangerLevel.High || !CanStunTarget(unit))
            {
                return;
            }

            var range = unit.Distance(ObjectManager.Player);
            if (range <= _e.Range)
            {
                if (_e.IsReady())
                {
                    _e.Cast(unit, _packetCast);
                }
            }
            else if (range <= _q.Range)
            {
                if (!_q.IsReady() || !_e.IsReady())
                {
                    return;
                }
                _q.Cast(unit, _packetCast);
                _e.Cast(unit, _packetCast);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = _menu.Item("qDraw").GetValue<bool>();
            var drawE = _menu.Item("eDraw").GetValue<bool>();
            var drawR = _menu.Item("rDraw").GetValue<bool>();
            var drawStunnable = _menu.Item("drawStunnable").GetValue<bool>();

            var position = ObjectManager.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(position, _q.Range, _q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(position, _e.Range, _e.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(position, _r.Range, _r.IsReady() ? Color.Aqua : Color.Red);
            }

            if (!drawStunnable)
            {
                return;
            }

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget()).Where(CanStunTarget))
            {
                const string text = "Stunnable";
                var size = Drawing.GetTextExtent(text);
                var enemyPos = Drawing.WorldToScreen(enemy.Position);

                Drawing.DrawText(enemyPos.X - size.Width / 2f, enemyPos.Y, Color.LightSeaGreen, text);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            _packetCast = _menu.Item("packetCast").GetValue<bool>();

            FireCharges();

            if (!Orbwalking.CanMove(100))
            {
                return;
            }

            if (_menu.Item("waveClear").GetValue<KeyBind>().Active && !ObjectManager.Player.IsDead)
            {
                WaveClear();
            }

            if (_menu.Item("comboActive").GetValue<KeyBind>().Active && !ObjectManager.Player.IsDead)
            {
                Combo();
            }

            if (_menu.Item("qLastHit").GetValue<KeyBind>().Active && _menu.Item("qLasthitEnable").GetValue<bool>() &&
                !ObjectManager.Player.IsDead)
            {
                LastHitWithQ();
            }
        }

        private static void WaveClear()
        {
            var useQ = _menu.Item("useQWC").GetValue<bool>();
            var useW = _menu.Item("useWWC").GetValue<bool>();
            var useR = _menu.Item("useRWC").GetValue<bool>();

            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, 650);
            foreach (var minion in minions)
            {
                if (useQ)
                {
                    if (_menu.Item("useQWCKillable").GetValue<bool>())
                    {
                        var damage = ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q);

                        if (damage > minion.Health && _q.IsReady())
                        {
                            _q.Cast(minion, _packetCast);
                        }
                    }
                    else
                    {
                        _q.Cast(minion, _packetCast);
                    }
                }

                if (useW && _w.IsReady())
                {
                    _w.Cast();
                }
                if (useR && _r.IsReady())
                {
                    _r.Cast(minion, _packetCast);
                }
            }
        }

        private static void LastHitWithQ()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range);
            foreach (var minion in
                minions.Where(minion => ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health))
            {
                var noFarmDangerous = _menu.Item("qNoFarmTower").GetValue<bool>();
                // If do not farm under tower
                if (noFarmDangerous)
                {
                    if (minion.UnderTurret())
                    {
                        continue;
                    }
                    if (_q.IsReady())
                    {
                        _q.Cast(minion, _packetCast);
                    }
                }
                else
                {
                    if (_q.IsReady())
                    {
                        _q.Cast(minion, _packetCast);
                    }
                }
            }
        }

        private static void FireCharges()
        {
            if (!_hasToFire)
            {
                return;
            }

            _r.Cast(Orbwalker.GetTarget() as Obj_AI_Hero, _packetCast); //Dunnno
            _charges -= 1;
            _hasToFire = _charges != 0;
        }

        private static void Combo()
        {
            // Simple combo q -> w -> e -> r
            var useQ = _menu.Item("useQ").GetValue<bool>();
            var useW = _menu.Item("useW").GetValue<bool>();
            var useE = _menu.Item("useE").GetValue<bool>();
            var useR = _menu.Item("useR").GetValue<bool>();
            var useEStun = _menu.Item("useEStun").GetValue<bool>();
            var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);

            if (target == null)
            {
                GapCloseCombo();
            }
            if (target == null || !target.IsValid)
            {
                return;
            }

            var isUnderTower = target.UnderTurret();
            var diveTower = _menu.Item("diveTower").GetValue<bool>();
            var doNotCombo = false;

            // if target is under tower, and we do not want to dive
            if (isUnderTower && !diveTower)
            {
                // Calculate percent hp
                var percent = (int) target.Health / target.MaxHealth * 100;
                var overridePercent = _menu.Item("diveTowerPercent").GetValue<Slider>().Value;

                if (percent > overridePercent)
                {
                    doNotCombo = true;
                }
            }

            if (doNotCombo)
            {
                return;
            }

            if (useW && _w.IsReady())
            {
                _w.Cast();
            }

            // follow up with q
            if (useQ && _q.IsReady())
            {
                if (_menu.Item("dontQ").GetValue<bool>())
                {
                    var distance = ObjectManager.Player.Distance(target);

                    if (distance > _menu.Item("dontQRange").GetValue<Slider>().Value)
                    {
                        _q.Cast(target, _packetCast);
                    }
                }
                else
                {
                    _q.Cast(target, _packetCast);
                }
            }

            // Now that we q'd, lets cast BOTRK
            if (BotRk.IsReady())
            {
                BotRk.Cast(target);
            }

            if (Cutlass.IsReady())
            {
                Cutlass.Cast(target);
            }

            // stunerino
            if (useE && _e.IsReady())
            {
                if (useEStun)
                {
                    if (CanStunTarget(target))
                    {
                        _e.Cast(target, _packetCast);
                    }
                }
                else
                {
                    _e.Cast(target, _packetCast);
                }
            }

            // Resharper did this, IDK if it works.
            // Original code:  if (useR && R.IsReady() && !hasToFire)
            if (!useR || !_r.IsReady() || _hasToFire)
            {
                return;
            }
            _hasToFire = true;
            _charges = 4;
        }

        private static void GapCloseCombo()
        {
            if (!_menu.Item("useMinionGapclose").GetValue<bool>())
            {
                return;
            }

            var target = TargetSelector.GetTarget(_q.Range * 3, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget() || target == null)
            {
                return;
            }

            foreach (var minion in
                MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range)
                    .Where(
                        minion =>
                            ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health &&
                            minion.ServerPosition.Distance(target.ServerPosition) < _q.Range)
                    .Where(minion => minion.IsValidTarget(_q.Range * 3))
                    .Where(minion => _q.IsReady()))
            {
                _q.Cast(minion, _packetCast);
                break;
            }
        }

        private static bool CanStunTarget(AttackableUnit target)
        {
            if (!_e.IsReady())
            {
                return false;
            }

            var enemyHealthPercent = target.Health / target.MaxHealth * 100;
            var myHealthPercent = ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100;

            return enemyHealthPercent > myHealthPercent;
        }

        private static void SetupMenu()
        {
            _menu = new Menu("ChewyMoon's Irelia", "cmIrelia", true);

            // Target Selector
            var targetSelectorMenu = new Menu("Target Selector", "cmIreliaTS");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _menu.AddSubMenu(targetSelectorMenu);

            // Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "cmIreliaOW");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            // Combo
            var comboMenu = new Menu("Combo", "cmIreliaCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useEStun", "Use e only if target can be stunned").SetValue(false));
            comboMenu.AddItem(new MenuItem("useR", "Use R in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useMinionGapclose", "Q minion gap closer").SetValue(true));
            _menu.AddSubMenu(comboMenu);

            // Lasthiting
            var farmingMenu = new Menu("Farming", "cmIreliaFarming");
            farmingMenu.AddItem(new MenuItem("qLasthitEnable", "Last hitting with Q").SetValue(false));
            farmingMenu.AddItem(
                new MenuItem("qLastHit", "Last hit with Q").SetValue(new KeyBind(88, KeyBindType.Press)));
            farmingMenu.AddItem(new MenuItem("qNoFarmTower", "Don't Q minions under tower").SetValue(false));
            // Wave clear submenu
            var waveClearMenu = new Menu("Wave Clear", "cmIreliaFarmingWaveClear");
            waveClearMenu.AddItem(new MenuItem("useQWC", "Use Q").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("useWWC", "Use W").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("useRWC", "Use R").SetValue(false));
            waveClearMenu.AddItem(new MenuItem("useQWCKillable", "Only Q killable minions").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("waveClear", "Wave Clear!").SetValue(new KeyBind(86, KeyBindType.Press)));
            farmingMenu.AddSubMenu(waveClearMenu);
            _menu.AddSubMenu(farmingMenu);

            //Drawing menu
            var drawingMenu = new Menu("Drawing", "cmIreliaDraw");
            drawingMenu.AddItem(new MenuItem("qDraw", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("eDraw", "Draw E").SetValue(false));
            drawingMenu.AddItem(new MenuItem("rDraw", "Draw R").SetValue(true));
            drawingMenu.AddItem(new MenuItem("comboDraw", "Draw Combo Damage").SetValue(true));
            drawingMenu.Item("comboDraw").ValueChanged +=
                (sender, args) => Utility.HpBarDamageIndicator.Enabled = args.GetNewValue<bool>();
            drawingMenu.AddItem(new MenuItem("drawStunnable", "Draw Stunnable").SetValue(true));
            _menu.AddSubMenu(drawingMenu);

            //Misc
            var miscMenu = new Menu(" Misc", "cmIreliaMisc");
            miscMenu.AddItem(new MenuItem("interruptUlts", "Interrupt ults with E").SetValue(true));
            miscMenu.AddItem(new MenuItem("interruptQE", "Q + E to interrupt if not in range").SetValue(true));
            miscMenu.AddItem(new MenuItem("packetCast", "Use packets to cast spells").SetValue(false));
            miscMenu.AddItem(new MenuItem("diveTower", "Dive tower when combo'ing").SetValue(false));
            miscMenu.AddItem(new MenuItem("diveTowerPercent", "Override dive tower").SetValue(new Slider(10)));
            miscMenu.AddItem(new MenuItem("dontQ", "Dont Q if range is small").SetValue(false));
            miscMenu.AddItem(new MenuItem("dontQRange", "Q Range").SetValue(new Slider(200, 0, 650)));
            miscMenu.AddItem(new MenuItem("eOnGapclose", "E on Gapcloser").SetValue(true));
            _menu.AddSubMenu(miscMenu);

            // Use combo, last hit, c
            _menu.AddItem(new MenuItem("comboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Finalize
            _menu.AddToMainMenu();
        }
    }
}