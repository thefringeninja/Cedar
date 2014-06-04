namespace Cedar.CommandHandling
{
    using System;
    using System.Linq;

    internal static class TypeExtensions
    {
        internal static Type[] GetInterfaceGenericTypeArguments(this Type type, Type interfaceType)
        {
            return type
                .GetInterfaces()
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                .GetGenericArguments();
        }

        internal static Type GetGenericInterfaceTypeDefinition(this Type type, Type interfaceType)
        {
            return type.GetInterfaces()
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }
    }
}