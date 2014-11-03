namespace Cedar.ProcessManagers.Persistence
{
    using System;
    using System.Reflection;

    public class DefaultProcessManagerFactory : IProcessManagerFactory
    {
        public IProcessManager Build(Type type, string id, string correlationId)
        {
            ConstructorInfo constructor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof(string), typeof(string)}, null);

            return (IProcessManager) constructor.Invoke(new object[] {id, correlationId});
        }
    }
}