namespace Cedar.Testing.TestRunner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public static class FindScenarios
    {
        public static IEnumerable<Func<KeyValuePair<string, Task<ScenarioResult>>>> InType(Type type)
        {
            return from method in type.GetMethods()
                let constructor=  type.GetConstructor(Type.EmptyTypes)
                where constructor != null
                      && method.ReturnType == typeof (Task<ScenarioResult>)
                select FromMethodInfo(method, constructor);
        }
        public static IEnumerable<Func<KeyValuePair<string, Task<ScenarioResult>>>> InAssemblies(params Assembly[] assemblies)
        {
            return from assembly in assemblies
                from type in assembly.GetTypes()
                from result in InType(type)
                select result;
        }

        private static Func<KeyValuePair<string, Task<ScenarioResult>>> FromMethodInfo(MethodInfo method, ConstructorInfo constructor)
        {
            return () =>
            {
                var instance = constructor.Invoke(new object[0]);

                return new KeyValuePair<string, Task<ScenarioResult>>(constructor.DeclaringType.FullName, (Task<ScenarioResult>) method.Invoke(instance, new object[0]));
            };
        }
    }
}