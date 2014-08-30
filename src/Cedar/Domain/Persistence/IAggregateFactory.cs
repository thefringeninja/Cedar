namespace Cedar.Domain.Persistence
{
    using System;

    public interface IAggregateFactory
    {
        IAggregate Build(Type type, string id);
    }
}