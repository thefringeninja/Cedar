namespace Cedar.ProcessManagers.Persistence
{
    using System;
    using System.Reflection;

    public class DefaultProcessManagerFactory : IProcessManagerFactory
    {
        public IProcessManager Build(Type type, string id)
        {
            ConstructorInfo constructor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof(string)}, null);

            return (IProcessManager) constructor.Invoke(new object[] {id});
        }
    }
}