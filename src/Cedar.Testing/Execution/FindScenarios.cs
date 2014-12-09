namespace Cedar.Testing.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public static class FindScenarios
    {
        public static IEnumerable<Func<KeyValuePair<Type, Task<ScenarioResult>>>> InAssemblies(params Assembly[] assemblies)
        {
            return from assembly in assemblies
                from type in assembly.GetTypes()
                from result in InType(type)
                select result;
        }

        private static IEnumerable<Func<KeyValuePair<Type, Task<ScenarioResult>>>> InType(Type type)
        {
            var constructor = type.GetConstructor(Type.EmptyTypes);
            
            if(constructor == null)
            {
                return Enumerable.Empty<Func<KeyValuePair<Type, Task<ScenarioResult>>>>();
            }

            var singles = from method in type.GetMethods()
                where method.ReturnType == typeof(Task<ScenarioResult>)
                select FromMethodInfo(method, constructor);

            var enumerables = from method in type.GetMethods()
                where typeof(IEnumerable<Task<ScenarioResult>>).IsAssignableFrom(method.ReturnType)
                from result in FromEnumerableMethodInfo(method, constructor)
                select result;

            return singles.Concat(enumerables);
        }

        private static Func<KeyValuePair<Type, Task<ScenarioResult>>> FromMethodInfo(MethodInfo method, ConstructorInfo constructor)
        {
            var instance = constructor.Invoke(new object[0]);

            var result = ((Task<ScenarioResult>)method.Invoke(instance, new object[0]));

            return () => new KeyValuePair<Type, Task<ScenarioResult>>(method.DeclaringType,
                result.ContinueWith(task =>
                {
                    DisposeIfNecessary(instance);

                    return task.Result;
                }));
        }

        private static IEnumerable<Func<KeyValuePair<Type, Task<ScenarioResult>>>> FromEnumerableMethodInfo(
            MethodInfo method, ConstructorInfo constructor)
        {
            var instance = constructor.Invoke(new object[0]);

            var results = (IEnumerable<Task<ScenarioResult>>)method.Invoke(instance, new object[0]);

            return results.Select(result => new Func<KeyValuePair<Type, Task<ScenarioResult>>>(
                () => new KeyValuePair<Type, Task<ScenarioResult>>(method.DeclaringType, result)));
        }


        private static void DisposeIfNecessary(object instance)
        {
            if(instance is IDisposable)
            {
                ((IDisposable) instance).Dispose();
            }
        }
    }
}