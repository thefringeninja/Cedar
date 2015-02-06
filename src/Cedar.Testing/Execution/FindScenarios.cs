namespace Cedar.Testing.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public static class FindScenarios
    {
        public static ILookup<Type, Task<ScenarioResult>> InAssemblies(params Assembly[] assemblies)
        {
            return (from assembly in assemblies
                from type in assembly.GetTypes()
                from result in InType(type)
                select new { type, result }).ToLookup(x => x.type, x => x.result);
        }

        private static IEnumerable<Task<ScenarioResult>> InType(Type type)
        {
            var constructor = type.GetConstructor(Type.EmptyTypes);

            if(constructor == null)
            {
                return Enumerable.Empty<Task<ScenarioResult>>();
            }

            var singles = from method in type.GetMethods()
                where method.ReturnType == typeof(Task<ScenarioResult>)
                from task in FromMethodInfo(method, constructor)
                select task;

            var enumerables = from method in type.GetMethods()
                where typeof(IEnumerable<Task<ScenarioResult>>).IsAssignableFrom(method.ReturnType)
                from task in FromEnumerableMethodInfo(method, constructor)
                select task;

            return singles.Concat(enumerables);
        }

        private static IEnumerable<Task<ScenarioResult>> FromMethodInfo(
            MethodInfo method,
            ConstructorInfo constructor)
        {
            var instance = constructor.Invoke(new object[0]);

            var task = (((Task<ScenarioResult>) method.Invoke(instance, new object[0])))
                .ContinueWith(t =>
                {
                    DisposeIfNecessary(instance);
                    return t.Result;
                });

            return new[] { task };
        }

        private static IEnumerable<Task<ScenarioResult>> FromEnumerableMethodInfo(
            MethodInfo method,
            ConstructorInfo constructor)
        {
            long refCount = 0;

            var instance = constructor.Invoke(new object[0]);

            var tasks = ((IEnumerable<Task<ScenarioResult>>) method.Invoke(instance, new object[0]))
                .Select(task =>
                {
                    Interlocked.Increment(ref refCount);

                    return task.ContinueWith(t =>
                    {
                        if(Interlocked.Decrement(ref refCount) == 0)
                        {
                            DisposeIfNecessary(instance);
                        }

                        return t.Result;
                    });
                });

            return tasks;
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