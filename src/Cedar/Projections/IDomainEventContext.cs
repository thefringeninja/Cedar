namespace Cedar.Projections
{
    using System.Collections.Generic;

    public interface IDomainEventContext
    {
        string AggregateRootId { get; }

        int Version { get; }

        IDictionary<string, object> EventHeaders { get; }

        IDictionary<string, object> CommitHeaders { get; }
    }
}