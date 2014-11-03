namespace Cedar.ProcessManagers.Persistence
{
    using System;

    public interface IProcessManagerFactory
    {
        IProcessManager Build(Type type, string id, string correlationId);
    }
}