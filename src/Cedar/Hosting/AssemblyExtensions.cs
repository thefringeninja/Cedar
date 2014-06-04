namespace Cedar.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets all non-abstract classes that immplement a generic interface i.e. IHandler&lt;&gt;
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="openGenericInterfaceType">The generic interface the class should implement.</param>
        /// <returns>An enumerable of types.</returns>
        public static IEnumerable<Type> GetImplementorsOfOpenGenericInterface(this Assembly assembly, Type openGenericInterfaceType)
        {
            Guard.Ensure(openGenericInterfaceType.IsInterface, "openGenericInterfaceType", "Type is not an interface");
            Guard.Ensure(openGenericInterfaceType.IsGenericType, "openGenericInterfaceType", "Type is not generic");

            return assembly
                .GetExportedTypes()
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && (t.GetInterfaces()
                                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterfaceType)));
        }

        /// <summary>
        /// Gets all non-abstract classes that immplement an interface i.e. IModule
        /// </summary>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>An enumerable of types.</returns>
        public static IEnumerable<Type> GetImplementorsOfInterface<T>(this Assembly assembly)
        {
            Guard.Ensure(typeof(T).IsInterface, "openGenericInterfaceType", "Type is not an interface");

            return assembly
                .GetExportedTypes()
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && (t.GetInterfaces()
                                .Any(i => i == typeof(T))));
        }


        /// <summary>
        /// Gets all inheritors of an open generic base type. i.e. AbstractBaseType&lt;&gt;
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="openGenericType">The open generic type</param>
        /// <returns>An enumerable of types.</returns>
        public static IEnumerable<Type> GetInheritorsOfOpenGenericType(this Assembly assembly, Type openGenericType)
        {
            Guard.Ensure(!openGenericType.IsInterface, "openGenericInterfaceType", "Type cannot be an interface");
            Guard.Ensure(openGenericType.IsGenericType, "openGenericInterfaceType", "Type must be generic");

            return assembly
                .GetExportedTypes()
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && t.BaseType != null
                            && (t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == openGenericType));
        }
    }
}
