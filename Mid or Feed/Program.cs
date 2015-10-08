#region



#endregion

namespace Mid_or_Feed
{
    using System;
    using System.Reflection.Emit;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Program
    {
        #region Methods

        /// <summary>
        ///     Currents the domain on unhandled exception.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="unhandledExceptionEventArgs">
        ///     The <see cref="UnhandledExceptionEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        private static void CurrentDomainOnUnhandledException(
            object sender,
            UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Console.WriteLine(((Exception)unhandledExceptionEventArgs.ExceptionObject).Message);
            Plugin.PrintChat("encountered an error! (This error may have been caused by another assembly)");
        }

        /// <summary>
        ///     Fired when the game loads.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            var plugin = Type.GetType("Mid_or_Feed.Champions." + ObjectManager.Player.ChampionName);

            if (plugin == null)
            {
                Plugin.PrintChat(ObjectManager.Player.ChampionName + " not supported!");
                return;
            }

            DynamicInitializer.NewInstance(plugin);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        /// <summary>
        ///     The entry point of the executable.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion
    }

    /// <summary>
    ///     Dynamicly creates objects.
    /// </summary>
    public class DynamicInitializer
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <typeparam name="TV">The type of the v.</typeparam>
        /// <returns></returns>
        public static TV NewInstance<TV>() where TV : class
        {
            return ObjectGenerator(typeof(TV)) as TV;
        }

        /// <summary>
        ///     Creates a new Instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object NewInstance(Type type)
        {
            return ObjectGenerator(type);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Generates an object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static object ObjectGenerator(Type type)
        {
            var target = type.GetConstructor(Type.EmptyTypes);
            var dynamic = new DynamicMethod(string.Empty, type, new Type[0], target.DeclaringType);
            var il = dynamic.GetILGenerator();
            il.DeclareLocal(target.DeclaringType);
            il.Emit(OpCodes.Newobj, target);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));
            return method();
        }

        #endregion
    }
}