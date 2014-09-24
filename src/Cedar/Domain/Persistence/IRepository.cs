namespace Cedar.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository
    {
        TAggregate GetById<TAggregate>(string bucketId, Guid id, int version) where TAggregate : class, IAggregate;

        Task Save(string bucketId, IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders);
    }
}