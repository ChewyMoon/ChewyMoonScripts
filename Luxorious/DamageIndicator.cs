namespace Luxorious
{
    using System;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     Indicated damage.
    /// </summary>
    public class DamageIndicator
    {
        #region Constants

        /// <summary>
        /// The height
        /// </summary>
        private const int Height = 8;

        /// <summary>
        /// The width
        /// </summary>
        private const int Width = 103;

        /// <summary>
        /// The x offset
        /// </summary>
        private const int XOffset = 10;

        /// <summary>
        /// The y offset
        /// </summary>
        private const int YOffset = 20;

        #endregion

        #region Static Fields

        /// <summary>
        /// The color
        /// </summary>
        public static Color Color = Color.Lime;

        /// <summary>
        /// If this instance is enabled or not.
        /// </summary>
        public static bool Enabled = true;

        /// <summary>
        /// <c>true</c> to fill the HP bar, else <c>false</c>
        /// </summary>
        public static bool Fill = true;

        /// <summary>
        /// The fill color
        /// </summary>
        public static Color FillColor = Color.Goldenrod;

        /// <summary>
        /// The text
        /// </summary>
        private static readonly Render.Text Text = new Render.Text(0, 0, "", 14, SharpDX.Color.Red, "monospace");

        #endregion

        #region Delegates

        /// <summary>
        /// Gets damage done to a target.
        /// </summary>
        /// <param name="hero">The hero.</param>
        /// <returns></returns>
        public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DamageIndicator"/> class.
        /// </summary>
        static DamageIndicator()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets the damage to the unit.
        /// </summary>
        /// <value>
        /// The damage to the unit.
        /// </value>
        public static DamageToUnitDelegate DamageToUnit { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Fired when the game is drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Enabled || DamageToUnit == null)
            {
                return;
            }

            foreach (var unit in HeroManager.Enemies.Where(h => h.IsValid && h.IsHPBarRendered))
            {
                var barPos = unit.HPBarPosition;
                var damage = DamageToUnit(unit);
                var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                var yPos = barPos.Y + YOffset;
                var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                var xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                if (damage > unit.Health)
                {
                    Text.X = (int)barPos.X + XOffset;
                    Text.Y = (int)barPos.Y + YOffset - 13;
                    Text.text = "Killable: " + (unit.Health - damage);
                    Text.OnEndScene();
                }

                Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 1, Color);

                if (!Fill)
                {
                    continue;
                }

                var differenceInHp = xPosCurrentHp - xPosDamage;
                var pos1 = barPos.X + 9 + (107 * percentHealthAfterDamage);

                for (var i = 0; i < differenceInHp; i++)
                {
                    Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, FillColor);
                }
            }
        }

        #endregion
    }
}