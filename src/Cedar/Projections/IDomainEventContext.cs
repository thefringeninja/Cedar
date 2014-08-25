namespace Cedar.Projections
{
    using System.Collections.Generic;
    using NEventStore;

    public interface IDomainEventContext
    {
        ICommit Commit { get; }

        int Version { get; }

        IDictionary<string, object> EventHeaders { get; }
    }
}