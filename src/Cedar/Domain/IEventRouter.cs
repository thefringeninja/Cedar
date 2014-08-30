namespace Cedar.Domain
{
    using System;

    public interface IEventRouter
    {
        void Register<T>(Action<T> handler);

        void Register(IAggregate aggregate);

        void Dispatch(object eventMessage);
    }
}