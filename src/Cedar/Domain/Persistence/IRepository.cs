namespace Cedar.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRepository
    {
        Task<TAggregate> GetById<TAggregate>(string bucketId, Guid id, int version, CancellationToken cancellationToken)
            where TAggregate : class, IAggregate;

        Task<TAggregate> GetById<TAggregate>(string bucketId, string id, int version, CancellationToken cancellationToken)
            where TAggregate : class, IAggregate;

        Task Save(
            string bucketId,
            IAggregate aggregate,
            Guid commitId,
            Action<IDictionary<string, object>> updateHeaders,
            CancellationToken cancellationToken);
    }
}