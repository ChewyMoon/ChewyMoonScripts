#region



#endregion

namespace Mid_or_Feed
{
    using LeagueSharp.Common;

    /// <summary>
    ///     Represents a manager.
    /// </summary>
    internal abstract class Manager
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Loads the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public virtual void Load(Menu config)
        {
        }

        #endregion
    }
}