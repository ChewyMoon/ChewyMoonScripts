namespace TeamFightCalculator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;
    using SharpDX.Direct3D9;

    class Program
    {
        #region Static Fields

        /// <summary>
        ///     The calculated slots
        /// </summary>
        static readonly SpellSlot[] CalculatedSlots = { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };

        /// <summary>
        ///     The ally damage
        /// </summary>
        static double allyDamage;

        /// <summary>
        ///     The enemy damage
        /// </summary>
        static double enemyDamage;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the allies.
        /// </summary>
        /// <value>
        ///     The allies.
        /// </value>
        static IEnumerable<Obj_AI_Hero> Allies { get; set; }

        /// <summary>
        ///     Gets or sets the box line.
        /// </summary>
        /// <value>
        ///     The box line.
        /// </value>
        static Line BoxLine { get; set; }

        /// <summary>
        ///     Gets the calculate range.
        /// </summary>
        /// <value>
        ///     The calculate range.
        /// </value>
        static int CalculateRange
        {
            get
            {
                return Menu.Item("CalculateRange").GetValue<Slider>().Value;
            }
        }

        /// <summary>
        ///     Gets or sets the enemies.
        /// </summary>
        /// <value>
        ///     The enemies.
        /// </value>
        static IEnumerable<Obj_AI_Hero> Enemies { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        static Menu Menu { get; set; }

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        static CalcStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets the text font.
        /// </summary>
        /// <value>
        ///     The text font.
        /// </value>
        static Font TextFont { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        static Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the box is being dragged.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the box is being dragged; otherwise, <c>false</c>.
        /// </value>
        static bool BeingDragged { get; set; }

        const int Width = 250;

        const int Height = 100;

        static bool MouseOverBox { get { return new Rectangle((int)Position.X, (int)Position.Y, Width, Height).Contains(Utils.GetCursorPos());} }

        static Vector2 lastMousePos;

        #endregion

        #region Methods

        private static void CreateMenu()
        {
            Menu = new Menu("Team Fight Calculator", "cmTFC", true);
            Menu.AddItem(new MenuItem("CalculateRange", "Calculate Range").SetValue(new Slider(0x5DC, 0x9C4, 0x1388)));
            Menu.AddItem(new MenuItem("DrawCalculation", "Draw Calculation").SetValue(true));
            Menu.AddItem(new MenuItem("DrawRange", "Draw Calculation Range").SetValue(true));
            Menu.AddToMainMenu();
        }

        /// <summary>
        ///     Fired when the game is drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Status == CalcStatus.NoEnemies || !Menu.Item("DrawCalculation").IsActive())
            {
                //return;
            }

            BoxLine.Width = 100;
            BoxLine.Begin();
            BoxLine.Draw(new []{Position, Position + new Vector2(250, 0)}, new ColorBGRA(0, 0, 0, 256 / 2));
            BoxLine.End();

            
        }

        /// <summary>
        ///     Fired when the game is loaded.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            CreateMenu();

            var heroes = ObjectManager.Get<Obj_AI_Hero>();
            var objAiHeroes = heroes as Obj_AI_Hero[] ?? heroes.ToArray();

            Allies = objAiHeroes.Where(x => x.IsAlly);
            Enemies = objAiHeroes.Where(x => x.IsEnemy);

            Status = CalcStatus.NoEnemies;

            BoxLine = new Line(Drawing.Direct3DDevice);
            TextFont = new Font(
                Drawing.Direct3DDevice,
                new FontDescription()
                    {
                        FaceName = "Tahoma", Height = 14, OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Antialiased
                    });

            Position = new Vector2(100, 200);

            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += DrawingOnOnPostReset;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnUpdate += GameOnOnUpdate;
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            // TODO save position

            if (BeingDragged)
            {
                var newPos = Utils.GetCursorPos();
                Position += newPos - lastMousePos;
                lastMousePos = newPos;
            }

            if (args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN && MouseOverBox)
            {
                BeingDragged = true;
                lastMousePos = Utils.GetCursorPos();
            }

            if (args.Msg == (uint)WindowsMessages.WM_LBUTTONUP && BeingDragged)
            {
                BeingDragged = false;
            }

        }

        /// <summary>
        /// Fired when the DirectX device is reset.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void DrawingOnOnPostReset(EventArgs args)
        {
            BoxLine.OnResetDevice();
            TextFont.OnResetDevice();
        }

        /// <summary>
        /// Fired when the DirectX device is lost.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void Drawing_OnPreReset(EventArgs args)
        {
            BoxLine.OnLostDevice();
            TextFont.OnLostDevice();
        }

        /// <summary>
        ///     Fired when the game is updated.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnUpdate(EventArgs args)
        {
            var enemies = Enemies.Where(x => x.IsValidTarget(CalculateRange));
            var allies = Allies.Where(x => x.IsValidTarget(CalculateRange));

            allyDamage = 0;
            enemyDamage = 0;

            var enemyHeroes = enemies as Obj_AI_Hero[] ?? enemies.ToArray();
            if (!enemyHeroes.Any())
            {
                Status = CalcStatus.NoEnemies;
                return;
            }

            Parallel.ForEach(
                enemyHeroes,
                x =>
                enemyDamage +=
                x.GetComboDamage(new Obj_AI_Base(), CalculatedSlots) + x.GetAutoAttackDamage(new Obj_AI_Base()));

            Parallel.ForEach(
                allies,
                x =>
                allyDamage +=
                x.GetComboDamage(new Obj_AI_Base(), CalculatedSlots) + x.GetAutoAttackDamage(new Obj_AI_Base()));

            Status = CalcStatus.Calculated;
        }

        /// <summary>
        ///     The entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion
    }

    /// <summary>
    ///     The status of the calculation
    /// </summary>
    enum CalcStatus
    {
        /// <summary>
        ///     There are no enemies.
        /// </summary>
        NoEnemies,

        /// <summary>
        ///     The calculation was calculated
        /// </summary>
        Calculated
    }
}