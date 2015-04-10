#region

using System;
using System.Reflection.Emit;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Mid_or_Feed
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

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

        private static void CurrentDomainOnUnhandledException(object sender,
            UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Console.WriteLine(((Exception) unhandledExceptionEventArgs.ExceptionObject).Message);
            Plugin.PrintChat("encountered an error! (This error may have been caused by another assembly)");
        }
    }

    public class DynamicInitializer
    {
        public static TV NewInstance<TV>() where TV : class
        {
            return ObjectGenerator(typeof(TV)) as TV;
        }

        public static object NewInstance(Type type)
        {
            return ObjectGenerator(type);
        }

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
    }
}