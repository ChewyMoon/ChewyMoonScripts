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
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace BoxxyRiven
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.Permissions;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        #region Methods

        /// <summary>
        /// Gets or sets the animation time.
        /// </summary>
        /// <value>
        /// The animation time.
        /// </value>
        public static float AnimationTime { get; set; }

        /// <summary>
        /// Gets or sets the wind up time.
        /// </summary>
        /// <value>
        /// The wind up time.
        /// </value>
        public static float WindUpTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [being aa].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [being aa]; otherwise, <c>false</c>.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        public static bool BeingAA { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [being q].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [being q]; otherwise, <c>false</c>.
        /// </value>
        public static bool BeingQ { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [being w].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [being w]; otherwise, <c>false</c>.
        /// </value>
        public static bool BeingW { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [being e].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [being e]; otherwise, <c>false</c>.
        /// </value>
        public static bool BeingE { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [start full combo].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [start full combo]; otherwise, <c>false</c>.
        /// </value>
        public static bool StartFullCombo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [start full combo2].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [start full combo2]; otherwise, <c>false</c>.
        /// </value>
        public static bool StartFullCombo2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [start full combo3].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [start full combo3]; otherwise, <c>false</c>.
        /// </value>
        public static bool StartFullCombo3 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [after combo].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [after combo]; otherwise, <c>false</c>.
        /// </value>
        public static bool AfterCombo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can turn.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can turn; otherwise, <c>false</c>.
        /// </value>
        public static bool CanTurn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can move.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can move; otherwise, <c>false</c>.
        /// </value>
        public static bool CanMove { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can aa.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can aa; otherwise, <c>false</c>.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        public static bool CanAA { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can q.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can q; otherwise, <c>false</c>.
        /// </value>
        public static bool CanQ { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can w.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can w; otherwise, <c>false</c>.
        /// </value>
        public static bool CanW { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can e.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can e; otherwise, <c>false</c>.
        /// </value>
        public static bool CanE { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can sr.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can sr; otherwise, <c>false</c>.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        public static bool CanSR { get; set; }

        /// <summary>
        /// Gets or sets the fc damage.
        /// </summary>
        /// <value>
        /// The fc damage.
        /// </value>
        // ReSharper disable once InconsistentNaming
        public static Dictionary<int, double> FCDamage { get; set; }

        /// <summary>
        /// Gets or sets the last rc.
        /// </summary>
        /// <value>
        /// The last rc.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        public static float LastRC { get; set; }

        /// <summary>
        /// Gets or sets the last aa.
        /// </summary>
        /// <value>
        /// The last aa.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        public static float LastAA { get; set; }

        /// <summary>
        /// Gets or sets the last p.
        /// </summary>
        /// <value>
        /// The last p.
        /// </value>
        public static float LastP { get; set; }

        /// <summary>
        /// Gets or sets the last q.
        /// </summary>
        /// <value>
        /// The last q.
        /// </value>
        public static float LastQ { get; set; }

        /// <summary>
        /// Gets or sets the last q2.
        /// </summary>
        /// <value>
        /// The last q2.
        /// </value>
        public static float LastQ2 { get; set; }

        /// <summary>
        /// Gets or sets the last w.
        /// </summary>
        /// <value>
        /// The last w.
        /// </value>
        public static float LastW { get; set; }

        /// <summary>
        /// Gets or sets the last e.
        /// </summary>
        /// <value>
        /// The last e.
        /// </value>
        public static float LastE { get; set; }

        /// <summary>
        /// Gets or sets the last fr.
        /// </summary>
        /// <value>
        /// The last fr.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        public static float LastFR { get; set; }

        /// <summary>
        /// Gets or sets the last draw.
        /// </summary>
        /// <value>
        /// The last draw.
        /// </value>
        public static float LastDraw { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Program"/> is recall.
        /// </summary>
        /// <value>
        ///   <c>true</c> if recall; otherwise, <c>false</c>.
        /// </value>
        public static bool Recall { get; set; }

        /// <summary>
        /// Gets or sets the q.
        /// </summary>
        /// <value>
        /// The q.
        /// </value>
        public static Spell Q { get; set; }

        /// <summary>
        /// Gets or sets the w.
        /// </summary>
        /// <value>
        /// The w.
        /// </value>
        public static Spell W { get; set; }

        /// <summary>
        /// Gets or sets the e.
        /// </summary>
        /// <value>
        /// The e.
        /// </value>
        public static Spell E { get; set; }

        /// <summary>
        /// Gets or sets the r.
        /// </summary>
        /// <value>
        /// The r.
        /// </value>
        public static Spell R { get; set; }

        /// <summary>
        /// Gets or sets the ignite.
        /// </summary>
        /// <value>
        /// The ignite.
        /// </value>
        public static Spell Ignite { get; set; }

        /// <summary>
        /// Gets or sets the smite.
        /// </summary>
        /// <value>
        /// The smite.
        /// </value>
        public static Spell Smite { get; set; }

        /// <summary>
        /// Gets or sets the flash.
        /// </summary>
        /// <value>
        /// The flash.
        /// </value>
        public static Spell Flash { get; set; }

        /// <summary>
        /// Gets the player.
        /// </summary>
        /// <value>
        /// The player.
        /// </value>
        public static Obj_AI_Hero Player { get
        {
            return ObjectManager.Player;
        } }

        /// <summary>
        /// Gets or sets the passive stacks.
        /// </summary>
        /// <value>
        /// The passive stacks.
        /// </value>
        public static int PassiveStacks { get; set; }

        /// <summary>
        /// Gets or sets the tiamat.
        /// </summary>
        /// <value>
        /// The tiamat.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static Items.Item Tiamat { get; set; }

        /// <summary>
        /// Gets or sets the hydra.
        /// </summary>
        /// <value>
        /// The hydra.
        /// </value>
        public static Items.Item Hydra { get; set; }

        /// <summary>
        /// Gets or sets the youmuu.
        /// </summary>
        /// <value>
        /// The youmuu.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static Items.Item Youmuu { get; set; }

        /// <summary>
        /// Gets or sets the cutlass.
        /// </summary>
        /// <value>
        /// The cutlass.
        /// </value>
        public static Items.Item Cutlass { get; set; }

        /// <summary>
        /// Gets or sets the botrk.
        /// </summary>
        /// <value>
        /// The botrk.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static Items.Item Botrk { get; set; }

        /// <summary>
        /// Gets or sets the stalker.
        /// </summary>
        /// <value>
        /// The stalker.
        /// </value>
        public static Items.Item Stalker { get; set; }

        /// <summary>
        /// Gets or sets the stalker w.
        /// </summary>
        /// <value>
        /// The stalker w.
        /// </value>
        public static Items.Item StalkerW { get; set; }

        /// <summary>
        /// Gets or sets the stalker m.
        /// </summary>
        /// <value>
        /// The stalker m.
        /// </value>
        public static Items.Item StalkerM { get; set; }

        /// <summary>
        /// Gets or sets the stalker j.
        /// </summary>
        /// <value>
        /// The stalker j.
        /// </value>
        public static Items.Item StalkerJ { get; set; }

        /// <summary>
        /// Gets or sets the stalker d.
        /// </summary>
        /// <value>
        /// The stalker d.
        /// </value>
        public static Items.Item StalkerD { get; set; }

        /// <summary>
        /// Gets or sets the minimum bounding box.
        /// </summary>
        /// <value>
        /// The minimum bounding box.
        /// </value>
        public static float MinBoundingBox { get; set; }

        /// <summary>
        /// Gets or sets the true range.
        /// </summary>
        /// <value>
        /// The true range.
        /// </value>
        public static float TrueRange { get; set; }

        /// <summary>
        /// Gets or sets the true target range.
        /// </summary>
        /// <value>
        /// The true target range.
        /// </value>
        public static float TrueTargetRange { get; set; }

        /// <summary>
        /// Gets or sets the target add range.
        /// </summary>
        /// <value>
        /// The target add range.
        /// </value>
        public static float TargetAddRange { get; set; }

        /// <summary>
        /// Gets or sets the kill steal target add range.
        /// </summary>
        /// <value>
        /// The kill steal target add range.
        /// </value>
        public static float KillStealTargetAddRange
        {
            get; set; 
        }

        /// <summary>
        /// Gets or sets the true minion range.
        /// </summary>
        /// <value>
        /// The true minion range.
        /// </value>
        public static float TrueMinionRange { get; set; }

        /// <summary>
        /// Gets or sets the true jungle mob range.
        /// </summary>
        /// <value>
        /// The true jungle mob range.
        /// </value>
        public static float TrueJunglemobRange { get; set; }

        /// <summary>
        /// Gets or sets the focus jungle names.
        /// </summary>
        /// <value>
        /// The focus jungle names.
        /// </value>
        public static string[] FocusJungleNames { get; set; }

        /// <summary>
        /// Gets or sets the jungle mob names.
        /// </summary>
        /// <value>
        /// The jungle mob names.
        /// </value>
        public static string[] JungleMobNames { get; set; }

        /// <summary>
        /// Gets or sets the menu.
        /// </summary>
        /// <value>
        /// The menu.
        /// </value>
        public static Menu Menu { get; set; }

        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <value>
        /// The time.
        /// </value>
        public static int Time { get
        {
            return Environment.TickCount & int.MaxValue;
        } }

        /// <summary>
        /// Gets or sets a value indicating whether the ultimate is activated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the ultimate is activated; otherwise, <c>false</c>.
        /// </value>
        public static bool RActivated { get; set; }

        /// <summary>
        /// Gets or sets the q count.
        /// </summary>
        /// <value>
        /// The q count.
        /// </value>
        public static int QCount { get; set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public static Obj_AI_Hero Target { get; set; }

        /// <summary>
        /// Gets or sets the kill steal target.
        /// </summary>
        /// <value>
        /// The kill steal target.
        /// </value>
        public static Obj_AI_Hero KillStealTarget { get; set; }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <typeparam name="T">The type of object to get from the menu.</typeparam>
        /// <param name="submenu">The submenu.</param>
        /// <param name="item">The item.</param>
        /// <returns>The value from the menu.</returns>
        public static T GetItem<T>(string submenu, string item)
        {
            return Menu.SubMenu(submenu).Item(item).GetValue<T>();
        }

        /// <summary>
        /// Determines whether the specified submenu is enabled.
        /// </summary>
        /// <param name="submenu">The submenu.</param>
        /// <returns>Whether the specified submenu is enabled.</returns>
        public static bool IsEnabled(string submenu)
        {
            return Menu.SubMenu(submenu).Item("On").GetValue<bool>();
        }

        /// <summary>
        /// Determines whether the specified submenu is enabled.
        /// </summary>
        /// <param name="submenu">The submenu.</param>
        /// <param name="subMenu2">The second submenu.</param>
        /// <returns>Whether the specified submenu is enabled.</returns>
        public static bool IsEnabled(string submenu, string subMenu2)
        {
            return Menu.SubMenu(submenu).SubMenu(subMenu2).Item("On").GetValue<bool>();
        }

        /// <summary>
        /// Determines whether [is keybind enabled] [the specified submenu].
        /// </summary>
        /// <param name="submenu">The submenu.</param>
        /// <returns></returns>
        public static bool IsKeybindEnabled(string submenu)
        {
            return Menu.SubMenu(submenu).Item("On").GetValue<KeyBind>().Active;
        }

        /// <summary>
        /// Gets the combo setting.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The combo setting.</returns>
        public static bool GetComboSetting(string name)
        {
            return Menu.SubMenu("Combo").Item(name).GetValue<bool>();
        }

        /// <summary>
        /// Gets a value indicating whether [stalker ready].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [stalker ready]; otherwise, <c>false</c>.
        /// </value>
        public static bool StalkerReady
        {
            get
            {
                return Smite.Slot != SpellSlot.Unknown
                       && (Stalker.IsOwned() || StalkerW.IsOwned() || StalkerM.IsOwned() || StalkerJ.IsOwned()
                           || StalkerD.IsOwned()) && Smite.IsReady();
            } }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        /// <summary>
        /// Raises the <see cref="E:GameLoad" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void OnGameLoad(EventArgs args)
        {
            Debugger.Launch();

            // Spells
            Q = new Spell(SpellSlot.Q, 260);
            W = new Spell(SpellSlot.W, 250);
            E = new Spell(SpellSlot.E, 270);
            R = new Spell(SpellSlot.R, 900);
            Ignite = new Spell(Player.GetSpellSlot("summonerdot"), 600);
            Smite = new Spell(Player.GetSpellSlot("smite"), 760);
            Flash = new Spell(Player.GetSpellSlot("summonerflash"), 400);

            R.SetSkillshot(0.25f, (float)(25 * Math.PI / 180), 2200, false, SkillshotType.SkillshotCone);

            // Items
            Tiamat = new Items.Item(3077, 150);
            Hydra = new Items.Item(3074, 150);
            Youmuu = new Items.Item(3142);
            Cutlass = new Items.Item(3144, 450);
            Botrk = new Items.Item(3153, 450);
            Stalker = new Items.Item(3706);
            StalkerW = new Items.Item(3707);
            StalkerM = new Items.Item(3708);
            StalkerJ = new Items.Item(3709);
            StalkerD = new Items.Item(3710);

            MinBoundingBox = 39.44f;
            TrueRange = 125.5f + MinBoundingBox;
            TrueTargetRange = TrueRange + 100;

            TrueMinionRange = TrueRange + 100;
            TrueJunglemobRange = TrueRange + 100;

            FCDamage = new Dictionary<int, double>();

            AnimationTime = (float)(1 / 0.625);
            AfterCombo = true;
            CanMove = true;
            CanAA = true;
            CanQ = true;
            CanW = true;
            CanE = true;
            CanSR = true;

            switch (Game.MapId)
            {
                case GameMapId.SummonersRift:
                    FocusJungleNames = new string[]
                                           {
                                               "SRU_Baron12.1.1", "SRU_Blue1.1.1", "SRU_Blue7.1.1", "Sru_Crab15.1.1",
                                               "Sru_Crab16.1.1", "SRU_Dragon6.1.1", "SRU_Gromp13.1.1", "SRU_Gromp14.1.1",
                                               "SRU_Krug5.1.2", "SRU_Krug11.1.2", "SRU_Murkwolf2.1.1", "SRU_Murkwolf8.1.1",
                                               "SRU_Razorbeak3.1.1", "SRU_Razorbeak9.1.1", "SRU_Red4.1.1", "SRU_Red10.1.1"
                                           };

                    JungleMobNames = new string[]
                                         {
                                             "SRU_BlueMini1.1.2", "SRU_BlueMini7.1.2", "SRU_BlueMini21.1.3",
                                             "SRU_BlueMini27.1.3", "SRU_KrugMini5.1.1", "SRU_KrugMini11.1.1",
                                             "SRU_MurkwolfMini2.1.2", "SRU_MurkwolfMini2.1.3", "SRU_MurkwolfMini8.1.2",
                                             "SRU_MurkwolfMini8.1.3", "SRU_RazorbeakMini3.1.2", "SRU_RazorbeakMini3.1.3",
                                             "SRU_RazorbeakMini3.1.4", "SRU_RazorbeakMini9.1.2", "SRU_RazorbeakMini9.1.3",
                                             "SRU_RazorbeakMini9.1.4", "SRU_RedMini4.1.2", "SRU_RedMini4.1.3",
                                             "SRU_RedMini10.1.2", "SRU_RedMini10.1.3"
                                         };
                    break;
                case GameMapId.TwistedTreeline:
                    FocusJungleNames = new string[]
                                           {
                                               "TT_NWraith1.1.1", "TT_NGolem2.1.1", "TT_NWolf3.1.1", "TT_NWraith4.1.1",
                                               "TT_NGolem5.1.1", "TT_NWolf6.1.1", "TT_Spiderboss8.1.1"
                                           };

                    JungleMobNames = new string[]
                                         {
                                             "TT_NWraith21.1.2", "TT_NWraith21.1.3", "TT_NGolem22.1.2", "TT_NWolf23.1.2",
                                             "TT_NWolf23.1.3", "TT_NWraith24.1.2", "TT_NWraith24.1.3", "TT_NGolem25.1.1",
                                             "TT_NWolf26.1.2", "TT_NWolf26.1.3"
                                         };
                    break;
            }

            Menu = new Menu("BoxxyRiven", "BoxxyRiven_CM", true);

            var tsMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            var comboMenu = new Menu("Combo Settings", "Combo");        
            comboMenu.AddItem(new MenuItem("Q", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("QS", "Use Q to Stick to Target").SetValue(false));
            comboMenu.AddItem(new MenuItem("Blank", string.Empty));
            comboMenu.AddItem(new MenuItem("W", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("Blank1", string.Empty));
            comboMenu.AddItem(new MenuItem("E", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("E2", "Use E if Health Percent > %").SetValue(new Slider(0, 0, 100)));
            comboMenu.AddItem(new MenuItem("EAA", "Don't Use E if enemy is in AA range").SetValue(false));
            comboMenu.AddItem(new MenuItem("Blank2", string.Empty));
            comboMenu.AddItem(new MenuItem("R", "Use R Combo").SetValue(true));
            comboMenu.AddItem(
                new MenuItem("FR", "Use Active R (FR)").SetValue(
                    new StringList(new[] { "None", "Killable", "Max Damage or Killable", "Full Combo" }, 2)));
            comboMenu.AddItem(new MenuItem("SR", "Use Cast R (SR)").SetValue(new StringList(new [] { "None", "Killable", "Max Damage or Killable" }, 2)));
            comboMenu.AddItem(new MenuItem("Rearly", "Use Second R Early").SetValue(false));
            comboMenu.AddItem(new MenuItem("DontR", "Don't use SR if killable with Q or W").SetValue(false));
            comboMenu.AddItem(new MenuItem("Blank5", string.Empty));
            comboMenu.AddItem(new MenuItem("AutoR", "Use Cast R by Min Count").SetValue(true));
            comboMenu.AddItem(new MenuItem("Rmin", "Cast R Min Count").SetValue(new Slider(4, 2, 5)));
            comboMenu.AddItem(new MenuItem("Bank6", string.Empty));
            comboMenu.AddItem(new MenuItem("Item", "Use Item").SetValue(true));
            comboMenu.AddItem(new MenuItem("BRK1", "Use BotRK if Target HP < x%").SetValue(new Slider(40, 0, 100)));
            comboMenu.AddItem(new MenuItem("BRK2", "Use BotRK if My HP < x%").SetValue(new Slider(15, 0, 100)));
            comboMenu.AddItem(new MenuItem("On", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddSubMenu(comboMenu);

            var fullComboMenu = new Menu("Full Combo Settings", "FCombo");
            fullComboMenu.AddItem(
                new MenuItem("On", "Full Combo (ER F W>AA>Item>RQ)").SetValue(new KeyBind(84, KeyBindType.Press)));
            fullComboMenu.AddItem(new MenuItem("F", "Use Flash").SetValue(true));
            Menu.AddSubMenu(fullComboMenu);

            var clearMenu = new Menu("Clear Settings", "Clear");

            var lcMenu = new Menu("Lane Clear Settings", "Farm");
            lcMenu.AddItem(new MenuItem("Q", "Use Q").SetValue(true));
            lcMenu.AddItem(new MenuItem("W", "Use W").SetValue(true));
            lcMenu.AddItem(new MenuItem("E", "Use E").SetValue(true));
            lcMenu.AddItem(new MenuItem("TH", "Use Tiamat or Hydra").SetValue(true));
            lcMenu.AddItem(new MenuItem("THmin", "Use Item Min Count").SetValue(new Slider(3, 1, 6)));
            lcMenu.AddItem(new MenuItem("Blank", string.Empty));
            lcMenu.AddItem(new MenuItem("On", "Lane Clear").SetValue(new KeyBind(66, KeyBindType.Press)));
            clearMenu.AddSubMenu(lcMenu);

            var jcMenu = new Menu("Jungle Clear Settings", "JFarm");
            jcMenu.AddItem(new MenuItem("Q", "Use Q").SetValue(true));
            jcMenu.AddItem(new MenuItem("W", "Use W").SetValue(true));
            jcMenu.AddItem(new MenuItem("E", "Use E").SetValue(true));
            jcMenu.AddItem(new MenuItem("TH", "Use Tiamat or Hydra").SetValue(true));
            jcMenu.AddItem(new MenuItem("THmin", "Use Item Min Count").SetValue(new Slider(3, 1, 6)));
            jcMenu.AddItem(new MenuItem("Blank", string.Empty));
            jcMenu.AddItem(new MenuItem("On", "Jungle Clear").SetValue(new KeyBind(66, KeyBindType.Press)));
            clearMenu.AddSubMenu(jcMenu);

            Menu.AddSubMenu(clearMenu);

            var harassMenu = new Menu("Harass Settings", "Harass");
            harassMenu.AddItem(new MenuItem("Q", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("W", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("E", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("Item", "Use Items").SetValue(true));
            harassMenu.AddItem(new MenuItem("Blank", string.Empty));
            harassMenu.AddItem(new MenuItem("On", "Harass").SetValue(new KeyBind(67, KeyBindType.Press)));
            Menu.AddSubMenu(harassMenu);

            var lastHitMenu = new Menu("Last Hit Settings", "LastHit");
            lastHitMenu.AddItem(new MenuItem("Q", "Use Q").SetValue(false));
            lastHitMenu.AddItem(new MenuItem("W", "Use W").SetValue(false));
            lastHitMenu.AddItem(new MenuItem("Orbwalk", "Orbwalk").SetValue(true));
            lastHitMenu.AddItem(new MenuItem("Blank", string.Empty));
            lastHitMenu.AddItem(new MenuItem("On", "Last Hit").SetValue(new KeyBind(88, KeyBindType.Press)));
            Menu.AddSubMenu(lastHitMenu);

            var jungleStealMenu = new Menu("Jungle Steal Settings", "JSteal");
            jungleStealMenu.AddItem(new MenuItem("Q", "Use Q1, Q2").SetValue(true));
            jungleStealMenu.AddItem(new MenuItem("W", "Use W").SetValue(true));
            jungleStealMenu.AddItem(new MenuItem("QW", "Use QW").SetValue(true));
            if (Smite.Slot != SpellSlot.Unknown)
            {
                jungleStealMenu.AddItem(new MenuItem("S", "Use Smite").SetValue(true));
            }
            jungleStealMenu.AddItem(new MenuItem("Blank", string.Empty));
            jungleStealMenu.AddItem(new MenuItem("On", "Jungle Steal").SetValue(new KeyBind(88, KeyBindType.Press)));
            Menu.AddSubMenu(jungleStealMenu);

            var ksMenu = new Menu("KillSteal Settings", "KillSteal");
            ksMenu.AddItem(new MenuItem("Q", "Use Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("W", "Use W").SetValue(true));
            ksMenu.AddItem(new MenuItem("R", "Use Second R").SetValue(true));
            if (Ignite.Slot != SpellSlot.Unknown)
            {
                ksMenu.AddItem(new MenuItem("I", "Use Ignite").SetValue(true));
            }
            if (Smite.Slot != SpellSlot.Unknown)
            {
                ksMenu.AddItem(new MenuItem("S", "Use Stalkers Blade").SetValue(true));
            }
            ksMenu.AddItem(new MenuItem("BRK", "Use BotRK").SetValue(true));
            ksMenu.AddItem(new MenuItem("Blank", string.Empty));
            ksMenu.AddItem(new MenuItem("On", "Kill Steal").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            var autoCastMenu = new Menu("AutoCast Settings", "Auto");
            autoCastMenu.AddItem(new MenuItem("StackQ", "Auto Stack Q").SetValue(true));
            autoCastMenu.AddItem(new MenuItem("AutoW", "Auto W by Min Count")).SetValue(true);
            autoCastMenu.AddItem(new MenuItem("Wmin", "W Min Count").SetValue(new Slider(1, 1, 5)));
            autoCastMenu.AddItem(new MenuItem("DontW", "Don't use W if Enemy Under Tower").SetValue(true));
            autoCastMenu.AddItem(new MenuItem("AutoR", "Auto Cast R by Min Count").SetValue(true));
            autoCastMenu.AddItem(new MenuItem("Rmin", "Cast R Min Count").SetValue(new Slider(5, 1, 5)));
            if (Smite.Slot != SpellSlot.Unknown)
            {
                autoCastMenu.AddItem(
                    new MenuItem("AutoS", "Auto Smite").SetValue(new KeyBind(78, KeyBindType.Toggle, true)));
            }
            autoCastMenu.AddItem(new MenuItem("Blank", string.Empty));
            autoCastMenu.AddItem(new MenuItem("On", "AutoCast").SetValue(true));
            Menu.AddSubMenu(autoCastMenu);

            var fleeMenu = new Menu("Flee Settings", "Flee");
            fleeMenu.AddItem(
                new MenuItem("On", "Flee (Only Use Kill Steal)").SetValue(new KeyBind(71, KeyBindType.Press)));
            Menu.AddSubMenu(fleeMenu);

            var miscMenu = new Menu("Misc Settings", "Misc");
            miscMenu.AddItem(new MenuItem("STT", "Stick to Target").SetValue(true));
            miscMenu.AddItem(new MenuItem("STTR", "(Dis to Target from Mouse <= x").SetValue(new Slider(350, 0, 900)));
            Menu.AddSubMenu(miscMenu);

            var drawingMenu = new Menu("Draw Settings", "Draw");
            drawingMenu.AddItem(new MenuItem("Target", "Draw Target").SetValue(true));
            drawingMenu.AddItem(new MenuItem("AA", "Draw Attack Range").SetValue(true));
            drawingMenu.AddItem(new MenuItem("Q", "Draw Q Range").SetValue(false));
            drawingMenu.AddItem(new MenuItem("W", "Draw W Range").SetValue(true));
            drawingMenu.AddItem(new MenuItem("E", "Draw E Range").SetValue(false));
            drawingMenu.AddItem(new MenuItem("R", "Draw R Range").SetValue(true));
            drawingMenu.AddItem(new MenuItem("S", "Draw Smite Range").SetValue(true));
            drawingMenu.AddItem(new MenuItem("FCD", "Draw Full Combo Damage").SetValue(true));
            Menu.AddSubMenu(drawingMenu);

            Menu.AddToMainMenu();

            Game.PrintChat("Hello");

            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnPlayAnimation += ObjAiBaseOnOnPlayAnimation;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        /// <summary>
        /// Called when the game processes a spell.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs"/> instance containing the event data.</param>
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

           
            var spell = args.SData.Name;
            Game.PrintChat(spell);

            if (spell.Equals("recall"))
            {
                LastRC = Time;
                Recall = true;
            }

            if (!BeingQ && spell.Contains("RivenBasicAttack"))
            {
                if (PassiveStacks >= 1)
                {
                    PassiveStacks -= 1;
                }

                // TODO Animation Time & Wind Up
                BeingAA = true;
                CanAA = false;
                LastAA = Time;
                LastP = Time;
               ;
                
            }

            if (spell.Equals("RivenTriCleave"))
            {
                Game.PrintChat("Detect Q");
                if (PassiveStacks <= 2)
                {
                    PassiveStacks++;
                }

                if (QCount <= 1)
                {
                    LastQ2 = Time;
                    QCount++;
                }
                else if (QCount == 2)
                {
                    QCount = 0;
                }

                AfterCombo = true;
                BeingQ = true;
                CanMove = false;
                CanQ = false;
                LastQ = Time;
                LastP = Time;
                StartFullCombo = false;
                StartFullCombo2 = false;
                StartFullCombo3 = false;
            }

            if (spell.Equals("RivenMartyr"))
            {
                if (PassiveStacks <= 2)
                {
                    PassiveStacks++;
                }

                BeingW = true;
                CanW = false;
                LastW = Time;
                LastP = Time;
                StartFullCombo = false;
                StartFullCombo3 = true;
            }

            if (spell.Equals("RivenFeint"))
            {
                if (PassiveStacks <= 2)
                {
                    PassiveStacks++;
                }

                BeingE = true;
                CanE = false;
                LastE = Time;
                LastP = Time;
            }

            if (spell.Equals("RivenFengShuiEngine"))
            {
                if (PassiveStacks <= 2)
                {
                    PassiveStacks++;
                }

                LastFR = Time;
                LastP = Time;
                RActivated = true;
                StartFullCombo = true;
            }

            if (spell.Equals("rivenizunablade"))
            {
                if (PassiveStacks <= 2)
                {
                    PassiveStacks++;
                }

                CanSR = false;
                LastP = Time;
                RActivated = false;
                StartFullCombo2 = false;
                StartFullCombo3 = true;
            }
        }

        /// <summary>
        /// Called when a <see cref="Obj_AI_Base"/> plays an animation.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectPlayAnimationEventArgs"/> instance containing the event data.</param>
        private static void ObjAiBaseOnOnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (!args.Animation.Equals("Idle1") || CanAA || CanMove)
            {
                return;
            }

            BeingAA = false;
            CanAA = true;
            CanMove = true;
        }

        /// <summary>
        /// Called when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void Game_OnUpdate(EventArgs args)
        {

            Checks();
            GetTargets();

            //if (IsEnabled("KillSteal"))
            //{
            //    KillSteal();
            //}

            //if (IsEnabled("Auto"))
            //{
            //    Auto();
            //}

            if (IsKeybindEnabled("Combo"))
            {
                Combo();

                //if (IsEnabled("FCombo"))
              //  {
                    //Menu.SubMenu("FCombo").Item("On").SetValue(false);
               // }
            }

            if (IsKeybindEnabled("FCombo"))
            {
                FCombo();
            }

            //if (IsEnabled("Clear", "Farm"))
            //{
            //    Farm();

            //    if (IsEnabled("FCombo"))
            //    {
            //        Menu.SubMenu("FCombo").Item("On").SetValue(false);
            //    }
            //}

            //if (IsEnabled("Clear", "JFarm"))
            //{
            //    JFarm();

            //    if (IsEnabled("FCombo"))
            //    {
            //        Menu.SubMenu("FCombo").Item("On").SetValue(false);
            //    }
            //}

            //if (IsEnabled("JSteal"))
            //{
            //    JSteal();

            //    if (IsEnabled("FCombo"))
            //    {
            //        Menu.SubMenu("FCombo").Item("On").SetValue(false);
            //    }
            //}

            //if (IsEnabled("Harass"))
            //{
            //    Harass();

            //    if (IsEnabled("FCombo"))
            //    {
            //        Menu.SubMenu("FCombo").Item("On").SetValue(false);
            //    }
            //}

            //if (IsEnabled("LastHit"))
            //{
            //    LastHit();

            //    if (IsEnabled("FCombo"))
            //    {
            //        Menu.SubMenu("FCombo").Item("On").SetValue(false);
            //    }
            //}

            //if (IsEnabled("Flee"))
            //{
            //    Flee();

            //    if (IsEnabled("FCombo"))
            //    {
            //        Menu.SubMenu("FCombo").Item("On").SetValue(false);
            //    }
            //}

        }

        /// <summary>
        /// Does the full combo (Shy combo)
        /// </summary>
        private static void FCombo()
        {
            Orbwalk("FCombo");

            var paaTargetDamage = GetDamage("PAA", Target);
            var qTargetDamage = GetDamage("Q", Target);
            var wTargetdamage = GetDamage("W", Target);
            var fcrTargetDamage = GetDamage("FCR", Target);
            var raaTargetDamage = GetDamage("RAA", Target);
            var rqTargetDamage = GetDamage("RQ", Target);
            var rwTargetDamage = GetDamage("RW", Target);
            var rfcrTargetDamage = GetDamage("RFCR", Target);

            var sbTargetDamage = GetDamage("STALKER", Target);

            var fComboF = GetItem<bool>("FCombo", "F");

            if (Smite.IsReady() && sbTargetDamage >= Target.Health && Target.IsValidTarget(Smite.Range))
            {
                CastS(Target);
            }

            if (Q.IsReady() && W.IsReady() && E.IsReady() && R.IsReady())
            {
                AfterCombo = false;

                if (!RActivated)
                {
                    if (fComboF && Flash.IsReady() && !Target.IsValidTarget(E.Range + TrueTargetRange - 40)
                        && Target.IsValidTarget(E.Range + Flash.Range + MinBoundingBox + TargetAddRange - 50))
                    {
                        CastE(Target);
                        Utility.DelayAction.Add(200, CastFR);
                        Utility.DelayAction.Add(250, () => CastF(Target));
                    }
                    else if (!(fComboF && Flash.IsReady()) && Target.IsValidTarget(E.Range + TrueTargetRange - 50))
                    {
                        CastE(Target);
                        Utility.DelayAction.Add(250, CastFR);
                    }
                }
                else if (RActivated)
                {
                    if (fComboF && Flash.IsReady() && !Target.IsValidTarget(E.Range + TrueTargetRange - 50)
                        && Target.IsValidTarget(E.Range + Flash.Range + MinBoundingBox + TargetAddRange - 50))
                    {
                        CastE(Target);
                        Utility.DelayAction.Add(250, () => CastF(Target));
                    }
                    else if (!(fComboF && R.IsReady()) && Target.IsValidTarget(E.Range + TrueTargetRange - 50))
                    {
                        CastE(Target);
                    }
                }
            }

            if (!AfterCombo)
            {
                if (StartFullCombo && Target.IsValidTarget(W.Range))
                {
                    CastW();
                }

                if (Tiamat.IsReady() && !BeingAA && Target.IsValidTarget(Tiamat.Range + TargetAddRange))
                {
                    CastT();
                }
                else if (Hydra.IsReady() && !BeingAA && Target.IsValidTarget(Hydra.Range + TargetAddRange))
                {
                    CastH();
                }

                if (Youmuu.IsReady() && Target.IsValidTarget(TrueTargetRange))
                {
                    CastY();
                }

                if (Cutlass.IsReady() && Target.IsValidTarget(Cutlass.Range))
                {
                    CastBC(Target);
                }
                else if (Botrk.IsReady() && Target.IsValidTarget(Botrk.Range))
                {
                    CastBotrk(Target);
                }

                if (StartFullCombo2 && RActivated && CanSR)
                {
                    CastSR(Target.ServerPosition);
                }

                if (StartFullCombo3)
                {
                    CastQ(Target);
                }
            }
            else if (AfterCombo)
            {
                if (CanTurn && Target.IsValidTarget(TrueTargetRange - 50))
                {
                    var cancelPos = Player.ServerPosition
                                    + (Target.ServerPosition - Player.ServerPosition).Normalized() * -300;
                    MoveToPos(cancelPos);
                    CanTurn = true;
                }
                else if (CanTurn && !Target.IsValidTarget(TrueTargetRange - 50))
                {
                    var cancelPos = Player.ServerPosition
                                    + (Target.ServerPosition - Player.ServerPosition).Normalized() * 300;
                    MoveToPos(cancelPos);
                    CanTurn = false;
                }

                if (Tiamat.IsReady() && !BeingAA && Target.IsValidTarget(Tiamat.Range + TargetAddRange))
                {
                    CastT();
                }
                else if (Hydra.IsReady() && !BeingAA && Target.IsValidTarget(Hydra.Range + TargetAddRange))
                {
                    CastH();
                }

                if (Youmuu.IsReady() && Target.IsValidTarget(TrueTargetRange))
                {
                    CastY();
                }

                if (!(Q.IsReady() || W.IsReady() || E.IsReady()))
                {
                    return;
                }

                if (E.IsReady() && CanE)
                {
                    if (!Target.IsValidTarget(E.Range - TrueTargetRange + 50)
                        && Target.IsValidTarget(E.Range + TrueTargetRange - 50))
                    {
                        CastE(Target);
                    }
                }
            }
        }

        /// <summary>
        /// Casts the flash.
        /// </summary>
        /// <param name="target">The target.</param>
        private static void CastF(Obj_AI_Hero target)
        {
            Flash.Cast(target.ServerPosition);
        }

        /// <summary>
        /// Gets the targets.
        /// </summary>
        private static void GetTargets()
        {
            var original = TargetSelector.Mode;

            Target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            
            TargetSelector.Mode = TargetSelector.TargetingMode.LessCast;
            KillStealTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            TargetSelector.Mode = original;
        }

        /// <summary>
        /// Checks this instance.
        /// </summary>
        private static void Checks()
        {
            if (BeingAA && Time - LastAA >= WindUpTime + 3)
            {
                BeingAA = false;
                CanMove = true;
                CanQ = true;
                CanW = true;
                CanE = true;
                CanSR = true;
            }

            if (BeingQ && Time - LastQ >= 400)
            {
                BeingQ = false;
                CanMove = true;
                CanAA = true;
            }

            if (BeingW && Time - LastW >= 266.7)
            {
                BeingW = false;
                CanMove = true;
            }

            if (BeingE && Time - LastE >= 500)
            {
                BeingE = false;
                CanMove = true;
            }

            if (!CanMove && !(BeingAA || BeingQ || BeingW || BeingE) && Time - LastAA >= WindUpTime + 3)
            {
                CanMove = true;
            }

            if (!CanAA && !(BeingQ || BeingW || BeingE) && Time - LastAA >= AnimationTime + 3)
            {
                CanAA = true;
            }

            if (!CanW && !(BeingAA || BeingQ || BeingE) && W.IsReady())
            {
                CanW = true;
            }

            if (!CanE && !(BeingAA || BeingQ || BeingW) && E.IsReady())
            {
                CanE = true;
            }

            if (!CanSR && !BeingAA && RActivated && R.IsReady())
            {
                CanSR = true;
            }

            if (PassiveStacks != 0 && Time - LastP >= 5000)
            {
                PassiveStacks = 0;
            }

            if (QCount != 0 && Time - LastQ >= 4000)
            {
                QCount = 0;
            }

            if (RActivated && Time - LastFR >= 15000)
            {
                RActivated = false;
            }

            if (Recall && Time - LastRC >= 8500)
            {
                Recall = false;
            }

            if (RActivated)
            {
                Q.Width = 400;
                Q.Range = 325;
                W.Range = 270;
            }
            else
            {
                Q.Width = 200;
                Q.Range = 225;
                W.Range = 250;
            }

            MinBoundingBox = Player.Distance(Player.BBox.Minimum) / 2;
            TrueRange = Player.AttackRange + MinBoundingBox;

            if (Target != null)
            {
                var addRange = Target.BBox.Minimum.Distance(Target.ServerPosition) / 2;

                TargetAddRange = addRange;
                TrueTargetRange = TrueRange + addRange;
            }

            if (KillStealTarget != null)
            {
                var addRange = KillStealTarget.BBox.Minimum.Distance(Target.ServerPosition) / 2;

                KillStealTargetAddRange = addRange;
            }

            DrawKillable();
        }

        /// <summary>
        /// Draws the killable.
        /// </summary>
        private static void DrawKillable()
        {
            if (!Menu.SubMenu("Draw").Item("FCD").GetValue<bool>() || !(Time - LastDraw >= 0.1))
            {
                return;
            }

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1500)))
            {
                FCDamage[enemy.NetworkId] = GetFullComboDamage(enemy);
            }

            LastDraw = Time;
        }

        /// <summary>
        /// Gets the full combo damage.
        /// </summary>
        /// <param name="enemy">The enemy.</param>
        /// <param name="full">if set to <c>true</c> [full].</param>
        /// <returns>The full combo damage</returns>
        private static double GetFullComboDamage(Obj_AI_Hero enemy, bool full = false)
        {
            var passiveAutoDamage = GetDamage("PAA", enemy);
            var qTargetDamage = GetDamage("Q", enemy);
            var wTargetDamage = GetDamage("W", enemy);
            var fcrTargetDamage = GetDamage("FCR", enemy);

            var raaTargetDamage = GetDamage("RAA", enemy);
            var rqTargetDamage = GetDamage("RQ", enemy);
            var rwTargetDamage = GetDamage("RW", enemy);
            var rfcrTargetDamage = GetDamage("RFCR", enemy);

            var totalDamage = 0d;

            if (!(R.IsReady() || RActivated))
            {
                totalDamage = wTargetDamage + passiveAutoDamage + qTargetDamage * (3 - QCount) + fcrTargetDamage;

                if (full)
                {
                    totalDamage += 2 * passiveAutoDamage;
                }
            }
            else
            {
                totalDamage = rwTargetDamage + raaTargetDamage + rqTargetDamage * (3 - QCount) + rfcrTargetDamage;

                if (full)
                {
                    totalDamage += 2 * raaTargetDamage;
                }
            }

            return totalDamage;
        }

        /// <summary>
        /// Gets the damage.
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <param name="enemy">The enemy.</param>
        /// <returns>The damage</returns>
        public static double GetDamage(string spell, Obj_AI_Base enemy)
        {
            if (enemy == null)
            {
                return 0d;
            }

            var level = Player.Level;
            var totalDamage = Player.TotalAttackDamage;
            var rTotalDamage = 1.2 * totalDamage;
            var addDamage = Player.FlatPhysicalDamageMod;
            var rAddDamage = addDamage + 0.2 * totalDamage;
            var armorPen = Player.FlatArmorPenetrationMod;
            var armorPenPercent = Player.PercentArmorPenetrationMod;
            var magicPen = Player.FlatMagicPenetrationMod;
            var magicPenPercent = Player.PercentMagicPenetrationMod;

            var armor = Math.Max(0, enemy.Armor * armorPenPercent - armorPen);
            var armorPercent = armor / (100 + armor);
            var magicArmor = Math.Max(0, enemy.SpellBlock * magicPenPercent - magicPen);
            var magicArmorPercent = magicArmor / (100 + magicArmor);
            var enemyLossHealth = 1 - enemy.HealthPercent;

            if (spell.Equals("IGNITE"))
            {
                return 50 + 20 * level;
            }

            if (spell.Equals("SMITE"))
            {
                if (level <= 4)
                {
                    return 370 + 20 * level;
                }

                if (level <= 9)
                {
                    return 330 + 30 * level;
                }

                if (level <= 14)
                {
                    return 240 + 40 * level;
                }

                return 100 + 50 * level;
            }

            if (spell.Equals("STALKER"))
            {
                return 20 + 8 * level;
            }

            if (spell.Equals("BC"))
            {
                return 100 * (1 - magicArmorPercent);
            }

            var pureDamage = 0d;

            if (spell.Equals("BRK"))
            {
                pureDamage = Math.Max(100, 0.1 * enemy.MaxHealth);
            }
            else if (spell.Equals("AA"))
            {
                if (PassiveStacks >= 1)
                {
                    pureDamage = totalDamage + (20 + Math.Floor(level / 3d) * 5) * totalDamage / 100;
                }
                else
                {
                    pureDamage = totalDamage;
                }
            }
            else if (spell.Equals("PAA"))
            {
                pureDamage = totalDamage + (20 + Math.Floor(level / 3d) * 5) * totalDamage / 100;
            }
            else if (spell.Equals("RAA"))
            {
                pureDamage = totalDamage + (20 + Math.Floor(level / 3d) * 5) * rTotalDamage / 100;
            }
            else if (spell.Equals("Q"))
            {
                pureDamage = Q.IsReady() ? 20 * Q.Level - 10 + (0.05 * Q.Instance.Level + .35) * totalDamage : 0;
            }
            else if (spell.Equals("RQ"))
            {
                pureDamage = Q.IsReady() ? 20 * Q.Level - 10 + (0.05 * Q.Level + 0.35) * rTotalDamage : 0;
            }
            else if (spell.Equals("W"))
            {
                pureDamage = W.IsReady() ? 30 * W.Level + 20 + addDamage : 0;
            }
            else if (spell.Equals("RW"))
            {
                pureDamage = W.IsReady() ? 30 * W.Level + 20 + rAddDamage : 0;
            }
            else if (spell.Equals("R"))
            {
                pureDamage = R.IsReady()
                                 ? Math.Min(
                                     (40 * R.Level + 40 + 0.6 * addDamage) * (1 + enemyLossHealth * (8 / 3f)),
                                     120 * R.Level + 120 + 1.8 * addDamage) : 0;
            }
            else if (spell.Equals("RR"))
            {
                pureDamage = R.IsReady()
                                 ? Math.Min(
                                     (40 * R.Level + 40 + 0.06 * rAddDamage) * (1 + enemyLossHealth * (8 / 3f)),
                                     120 * R.Level + 120 + 1.8 * rAddDamage) : 0;
            }

            return pureDamage * (1 - armorPercent);
        }

        /// <summary>
        /// Does the combo
        /// </summary>
        private static void Combo()
        {
            Orbwalk("Combo");

            if (KillStealTarget == null)
            {
                return;
            }

            var comboItem = GetComboSetting("Item");
            var comboBrk1 = GetItem<Slider>("Combo", "BRK1").Value;
            var comboBrk2 = GetItem<Slider>("Combo", "BRK2").Value;
            var comboQ = GetComboSetting("Q");
            var comboQs = GetComboSetting("QS");
            var comboW = GetComboSetting("W");
            var comboE = GetComboSetting("E");
            var comboE2 = GetItem<Slider>("Combo", "E2").Value;
            var comboEaa = GetComboSetting("EAA");
            var comboR = GetComboSetting("R");
            var comboFr = GetItem<StringList>("Combo", "FR").SelectedIndex;
            var comboSr = GetItem<StringList>("Combo", "SR").SelectedIndex;
            var comboRearly = GetComboSetting("Rearly");
            var comboDontR = GetComboSetting("DontR");
            var comboAutoR = GetComboSetting("AutoR");

            var qTargetDamage = GetDamage("Q", Target);
            var wTargetDamage = GetDamage("W", Target);
            var rTargetDamage = GetDamage("R", Target);
            var qrTargetDamage = GetDamage("QR", Target);
            var wrTargetDamage = GetDamage("WR", Target);
            var qwrTargetDamage = GetDamage("QWR", Target);
            var rKsTargetDamage = GetDamage("R", KillStealTarget);
            var rrKsTargetDamage = GetDamage("RR", KillStealTarget);
            var sbKsTargetDamage = GetDamage("STALKER", KillStealTarget);
            var bcKsTargetDamage = GetDamage("BC", KillStealTarget);
            var brkKsTargetDamage = GetDamage("BRK", KillStealTarget);

            if (StalkerReady && comboItem && sbKsTargetDamage >= KillStealTarget.Health
                && KillStealTarget.IsValidTarget(Stalker.Range))
            {
                CastS(KillStealTarget);
            }

            if (Cutlass.IsReady() && comboItem && bcKsTargetDamage >= KillStealTarget.Health
                && KillStealTarget.IsValidTarget(Cutlass.Range))
            {
                Cutlass.Cast(KillStealTarget);
            }
            else if (Botrk.IsReady() && comboItem && brkKsTargetDamage >= KillStealTarget.Health
                     && KillStealTarget.IsValidTarget(Botrk.Range))
            {
                Botrk.Cast(KillStealTarget);
            }

            if (comboAutoR && KillStealTarget.IsValidTarget(R.Range))
            {
                CastR2(KillStealTarget, "Combo");
            }

            if (R.IsReady() && !RActivated && comboR && comboFr != 1)
            {
                if (!KillStealTarget.IsValidTarget(R.Range - R.Delay * KillStealTarget.MoveSpeed))
                {
                    if (comboFr == 2 && rrKsTargetDamage >= KillStealTarget.Health)
                    {
                        CastFR();
                    }
                    else if (comboFr == 3
                             && (rrKsTargetDamage >= KillStealTarget.Health || 25 >= KillStealTarget.HealthPercent))
                    {
                        CastFR();
                    }
                    else if (comboFr == 4 && GetFullComboDamage(KillStealTarget, true) >= KillStealTarget.Health)
                    {
                        if (KillStealTarget.IsValidTarget(TrueTargetRange - 50))
                        {
                            CastFR();
                        }
                        else if (!(Q.IsReady() && comboQ) && E.IsReady() && comboE
                                 && KillStealTarget.IsValidTarget(E.Range + TrueTargetRange - 50))
                        {
                            CastE(KillStealTarget);
                            CastFR();
                        }
                        else if (Q.IsReady() && comboQ && !(E.IsReady() && comboE)
                                 && KillStealTarget.IsValidTarget(Q.Range))
                        {
                            CastFR();
                            CastQ(KillStealTarget);
                        }
                        else if (Q.IsReady() && comboQ && E.IsReady() && comboE
                                 && KillStealTarget.IsValidTarget(Q.Range + E.Range - 50))
                        {
                            CastE(KillStealTarget);
                            CastFR();
                            Utility.DelayAction.Add(250, () => CastQ(KillStealTarget));
                        }
                    }
                }
            }
            else if (R.IsReady() && RActivated && comboR && comboSr != 1)
            {
                if (
                    KillStealTarget.IsValidTarget(
                        R.Range - (Player.Distance(KillStealTarget) / R.Speed) * KillStealTarget.MoveSpeed))
                {
                    if (comboSr == 2 && rKsTargetDamage >= KillStealTarget.Health)
                    {
                        CastSR(KillStealTarget.ServerPosition);
                    }
                    else if (comboSr == 3
                             && (rKsTargetDamage >= KillStealTarget.Health || 25 >= KillStealTarget.HealthPercent))
                    {
                        CastSR(KillStealTarget.ServerPosition);
                    }
                }

                if (Target.IsValidTarget(Q.Range))
                {
                    if (Q.IsReady() && comboQ && W.IsReady() && comboW && comboRearly
                        && rTargetDamage + qTargetDamage + wTargetDamage >= Target.Health)
                    {
                        CastSR(Target.ServerPosition);
                        Utility.DelayAction.Add(250, () => CastQ(Target));
                        Utility.DelayAction.Add(500, CastW);
                    }

                    if (!comboDontR)
                    {
                        if (Q.IsReady() && comboQ && W.IsReady() && comboW
                            && qTargetDamage + wTargetDamage + qwrTargetDamage >= Target.Health)
                        {
                            CastQ(Target);
                            Utility.DelayAction.Add(250, CastW);
                            Utility.DelayAction.Add(517, () => CastSR(Target.ServerPosition));
                        }
                        else if (Q.IsReady() && comboQ && qTargetDamage + qrTargetDamage >= Target.Health)
                        {
                            CastQ(Target);
                            Utility.DelayAction.Add(250, () => CastSR(Target.ServerPosition));
                        }
                        else if (W.IsReady() && comboW && wTargetDamage + wrTargetDamage >= Target.Health
                                 && Target.IsValidTarget(W.Range))
                        {
                            CastW();
                            Utility.DelayAction.Add(267, () => CastSR(Target.ServerPosition));
                        }
                    }
                    else
                    {
                        if (Q.IsReady() && comboQ && W.IsReady() && comboW
                            && qTargetDamage + wTargetDamage >= Target.Health)
                        {
                            CastQ(Target);
                            Utility.DelayAction.Add(250, CastW);
                        }
                        else if (Q.IsReady() && comboQ && qTargetDamage >= Target.Health)
                        {
                            CastQ(Target);
                        }
                        else if (W.IsReady() && comboW && wTargetDamage >= Target.Health && Target.IsValidTarget(W.Range))
                        {
                            CastW();
                        }
                    }
                }
            }

            if (Target == null)
            {
                return;
            }

            if (CanTurn && Target.IsValidTarget(TrueTargetRange - 50))
            {
                var cancelPos = Player.ServerPosition + (Target.Position - Player.ServerPosition).Normalized() * -300;
                MoveToPos(cancelPos);
                CanTurn = false;
            }
            else if (CanTurn && !Target.IsValidTarget(TrueTargetRange - 50))
            {
                var cancelPos = Player.ServerPosition
                                + (Target.ServerPosition - Player.ServerPosition).Normalized() * 300;
                MoveToPos(cancelPos);
                CanTurn = false;
            }

            if (Tiamat.IsReady() && comboItem && !BeingAA && Target.IsValidTarget(Tiamat.Range + TargetAddRange))
            {
                CastT();
            }
            else if (Hydra.IsReady() && comboItem && !BeingAA && Target.IsValidTarget(Hydra.Range + TargetAddRange))
            {
                CastH();
            }

            if (Youmuu.IsReady() && comboItem && Target.IsValidTarget(TrueTargetRange))
            {
                CastY();
            }

            if (Cutlass.IsReady() && comboItem && (comboBrk1 >= Target.HealthPercent || comboBrk2 >= Player.HealthPercent) && Target.IsValidTarget(Cutlass.Range))
            {
                CastBC(Target);
            }
            else if (Botrk.IsReady() && comboItem
                     && (comboBrk1 >= Target.HealthPercent || comboBrk2 >= Player.HealthPercent)
                     && Target.IsValidTarget(Botrk.Range))
            {
                CastBotrk(Target);
            }

            if (!(Q.IsReady() || W.IsReady() || E.IsReady()))
            {
                return;
            }

            if (E.IsReady() && comboE && comboE2 <= Player.HealthPercent && CanE)
            {
                if (!comboEaa && !Target.IsValidTarget(E.Range - TrueTargetRange + 50)
                    && Target.IsValidTarget(E.Range + TrueTargetRange - 50))
                {
                    CastE(Target);
                }
                else if (comboEaa && !Target.IsValidTarget(TrueTargetRange)
                         && Target.IsValidTarget(E.Range + TrueTargetRange - 50))
                {
                    CastE(Target);
                }
                else if (Q.IsReady() && comboQ && !Target.IsValidTarget(E.Range + TrueTargetRange - 50)
                         && Target.IsValidTarget(Q.Range + E.Range - 50))
                {
                    CastE(Target);
                }
            }

            if (W.IsReady() && comboW && CanW && Time - LastE >= 250 && Target.IsValidTarget(W.Range))
            {
                CastW();
            }


            if (Q.IsReady() && comboQ && CanQ && Time - LastE > 250 && Target.IsValidTarget(Q.Range))
            {
                CastQ(Target);
            }
            else if (Q.IsReady() && comboQ && Time - LastE >= 250 && !Target.IsValidTarget(TrueTargetRange)
                     && Target.IsValidTarget(Q.Range))
            {
                Q.Cast(Target);
            }
            else if (Q.IsReady() && comboQ && comboQs && Time - LastE >= 250 && !Target.IsValidTarget(TrueTargetRange)
                     && Target.IsValidTarget((3 - QCount) * Q.Range))
            {
                CastQ(Target);
            }
            
        }

        /// <summary>
        /// Casts the blade of the ruined king.
        /// </summary>
        /// <param name="target">The target.</param>
        private static void CastBotrk(Obj_AI_Base target)
        {
            Botrk.Cast(target);
        }

        /// <summary>
        /// Casts the bilgewater cutlass.
        /// </summary>
        /// <param name="target">The target.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        private static void CastBC(Obj_AI_Base target)
        {
            Cutlass.Cast(target);
        }

        /// <summary>
        /// Casts the youmuu.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private static void CastY()
        {
            Youmuu.Cast();
        }

        /// <summary>
        /// Casts the tiamat.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private static void CastT()
        {
            Tiamat.Cast();
        }

        /// <summary>
        /// Casts the hydra.
        /// </summary>
        private static void CastH()
        {
            Hydra.Cast();
        }

        /// <summary>
        /// Casts the w.
        /// </summary>
        private static void CastW()
        {
            W.Cast();
        }

        /// <summary>
        /// Casts the q.
        /// </summary>
        /// <param name="target">The kill steal target.</param>
        private static void CastQ(Obj_AI_Base target)
        {
            if (!GetItem<KeyBind>("Flee", "On").Active)
            {
                Utility.DelayAction.Add(100, () => CanTurn = true);
            }

            Q.Cast(target.ServerPosition);
            LastQ = Time;
        }

        /// <summary>
        /// Casts the e.
        /// </summary>
        /// <param name="target">The target.</param>
        private static void CastE(Obj_AI_Base target)
        {
            E.Cast(target.ServerPosition);
            LastE = Time;
        }

        /// <summary>
        /// Casts the fr.
        /// </summary>
        private static void CastFR()
        {
            R.Cast();
        }

        /// <summary>
        /// Casts the r2.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="state">The combo.</param>
        private static void CastR2(Obj_AI_Hero target, string state)
        {
            var prediction = R.GetPrediction(target, true);

            if (prediction.AoeTargetsHitCount >= GetItem<Slider>(state, "Rmin").Value)
            {
                if (prediction.Hitchance >= HitChance.High)
                {
                    CastSR(prediction.CastPosition);
                }
            }
        }

        private static void CastSR(Vector3 castPosition)
        {
            CanQ = false;
            CanW = false;
            CanE = false;

            R.Cast(castPosition);
        }

        /// <summary>
        /// Casts the stalkers blade.
        /// </summary>
        /// <param name="killStealTarget">The kill steal target.</param>
        private static void CastS(Obj_AI_Hero killStealTarget)
        {
            Smite.CastOnUnit(killStealTarget);
        }

        /// <summary>
        ///     Orbwalks with the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        private static void Orbwalk(string state)
        {
            // TODO FIRSTORDEFAULT
            if (IsKeybindEnabled("Flee"))
            {
                return;
            }

            if (CanAA && CanMove && !(BeingAA || BeingW || BeingE))
            {
                if (Target != null && (state.Equals("Combo") || state.Equals("FCombo") || state.Equals("Harass"))
                    && Target.IsValidTarget(TrueTargetRange))
                {
                    OrbCastAutoAttack(Target);
                }
                else if (state.Equals("Farm"))
                {
                    if (AllyMinionCount(R.Range) == 0)
                    {
                        foreach (
                            var minion in
                                from minion in
                                    MinionManager.GetMinions(
                                        Q.Range + E.Range + TrueMinionRange,
                                        MinionTypes.All,
                                        MinionTeam.Enemy,
                                        MinionOrderTypes.MaxHealth)
                                let addRange = minion.BBox.Minimum.Distance(minion.ServerPosition) / 2
                                let trueMinionRange = TrueRange + addRange
                                where minion.IsValidTarget(trueMinionRange)
                                select minion)
                        {
                            OrbCastAutoAttack(minion);
                            break;
                        }
                    }
                    else if (AllyMinionCount(R.Range) >= 1)
                    {
                        foreach (
                            var minion in
                                from minion in
                                    MinionManager.GetMinions(
                                        Q.Range + E.Range + TrueMinionRange,
                                        MinionTypes.All,
                                        MinionTeam.Enemy,
                                        MinionOrderTypes.MaxHealth)
                                let addRange = minion.BBox.Minimum.Distance(minion.ServerPosition) / 2
                                let trueMinionRange = TrueRange + addRange
                                where minion.IsValidTarget(trueMinionRange) && GetDamage("AA", minion) >= minion.Health
                                select minion)
                        {
                            OrbCastAutoAttack(minion);
                            break;
                        }

                        foreach (
                            var minion in
                                from minion in
                                    MinionManager.GetMinions(
                                        Q.Range + E.Range + TrueMinionRange,
                                        MinionTypes.All,
                                        MinionTeam.Enemy,
                                        MinionOrderTypes.MaxHealth)
                                let addRange = minion.BBox.Minimum.Distance(minion.ServerPosition) / 2
                                let trueMinionRange = TrueRange + addRange
                                let aaDamage = GetDamage("AA", minion)
                                where
                                    minion.Health >= aaDamage + 30 * AllyMinionCount(R.Range)
                                    && minion.IsValidTarget(trueMinionRange)
                                select minion)
                        {
                            OrbCastAutoAttack(minion);
                            break;
                        }
                    }
                }
                else if (state.Equals("JFarm"))
                {
                    foreach (
                        var junglemob in
                            from junglemob in
                                MinionManager.GetMinions(
                                    Q.Range + E.Range + TrueJunglemobRange,
                                    MinionTypes.All,
                                    MinionTeam.Neutral,
                                    MinionOrderTypes.MaxHealth)
                            let addRange = junglemob.BBox.Minimum.Distance(junglemob.ServerPosition) / 2
                            let trueJungleMobRange = TrueRange + addRange
                            where junglemob.IsValidTarget(TrueJunglemobRange, false)
                            select junglemob)
                    {
                        OrbCastAutoAttack(junglemob);
                        break;
                    }
                }
                else if (state.Equals("LastHit"))
                {
                    foreach (
                        var minion in
                            from minion in
                                MinionManager.GetMinions(
                                    Q.Range + E.Range + TrueMinionRange,
                                    MinionTypes.All,
                                    MinionTeam.Enemy,
                                    MinionOrderTypes.MaxHealth)
                            let addRange = minion.BBox.Minimum.Distance(minion.ServerPosition) / 2
                            let trueMinionRange = TrueRange + addRange
                            let aaDamage = GetDamage("AA", minion)
                            where aaDamage >= minion.Health && minion.IsValidTarget(TrueMinionRange)
                            select minion)
                    {
                        OrbCastAutoAttack(minion);
                        break;
                    }
                }
            }

            if (!CanMove)
            {
                return;
            }

            if (GetItem<bool>("Misc", "STT") && Target != null
                && (state.Equals("Combo") || state.Equals("FCombo") || state.Equals("Harass"))
                && Target.Distance(Game.CursorPos) <= GetItem<Slider>("Misc", "STTR").Value
                && !Target.IsValidTarget(100) && Target.IsValidTarget(TrueTargetRange))
            {
                MoveToPos(Target);
            }
            else
            {
                MoveToMouse();
            }
        }

        /// <summary>
        /// Moves to mouse.
        /// </summary>
        private static void MoveToMouse()
        {
            if (Player.Distance(Game.CursorPos) <= 100)
            {
                Player.IssueOrder(
                    GameObjectOrder.MoveTo,
                    Player.ServerPosition + (Game.CursorPos - Player.ServerPosition).Normalized() * 300);
            }
            else
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
        }

        /// <summary>
        /// Moves to position.
        /// </summary>
        /// <param name="target">The target.</param>
        private static void MoveToPos(Obj_AI_Base target)
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, target.ServerPosition);
        }

        /// <summary>
        /// Moves to position.
        /// </summary>
        /// <param name="position">The position.</param>
        private static void MoveToPos(Vector3 position)
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, position);
        }

        /// <summary>
        /// Gets the count of ally minions.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns>The number of ally minions in the range.</returns>
        public static int AllyMinionCount(float range)
        {
            return ObjectManager.Get<Obj_AI_Minion>().Count(x => x.IsAlly && MinionManager.IsMinion(x));
        }

        /// <summary>
        /// Casts the auto attack for the orbwalker.
        /// </summary>
        /// <param name="target">The target.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private static void OrbCastAutoAttack(Obj_AI_Base target)
        {
            CanMove = false;
            CastAutoAttack(target);
            CanAA = false;
            CanQ = false;
            CanW = false;
            CanE = false;
            CanSR = false;
            LastAA = Time;
        }

        /// <summary>
        /// Casts the automatic attack.
        /// </summary>
        /// <param name="target">The target.</param>
        private static void CastAutoAttack(GameObject target)
        {
            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
        }

        #endregion
    }
}