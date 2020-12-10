using System;
using System.Collections.Generic;
using System.Linq;

namespace JCS.Argon.Helpers
{
    /// <summary>
    /// Contains any reflection-related utilities
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Searches the currently loaded assemblies for implementations of a given interface type
        /// </summary>
        /// <typeparam name="T">The type to be search for</typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> LocateAllImplementors<T>()
        {
            var type = typeof(T);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
            return types;
        }

        /// <summary>
        /// Convenience method for the direct instantiation of a given type, with a supplied list of constructor
        /// arguments
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructorParams">A variable list of constructor arguments</param>
        /// <returns></returns>
        public static object? InstantiateType(Type type, params object[] constructorParams)
        {
            return  Activator.CreateInstance(type, args:constructorParams); 
        }
    }
}