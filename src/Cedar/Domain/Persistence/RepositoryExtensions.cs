namespace Cedar.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using NEventStore;

    public static class RepositoryExtensions
    {
        public static TAggregate GetById<TAggregate>([NotNull] this IRepository repository, Guid id)
            where TAggregate : class, IAggregate
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return GetById<TAggregate>(repository, Bucket.Default, id);
        }

        public static TAggregate GetById<TAggregate>([NotNull] this IRepository repository, Guid id, int version)
            where TAggregate : class, IAggregate
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return repository.GetById<TAggregate>(Bucket.Default, id, version);
        }

        public static TAggregate GetById<TAggregate>([NotNull] this IRepository repository, string bucketId, Guid id)
            where TAggregate : class, IAggregate
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            return repository.GetById<TAggregate>(bucketId, id, int.MaxValue);
        }

        public static Task Save([NotNull] this IRepository repository, IAggregate aggregate, Guid commitId)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return repository.Save(aggregate, commitId, a => { });
        }

        public static Task Save(
            [NotNull] this IRepository repository,
            IAggregate aggregate,
            Guid commitId,
            Action<IDictionary<string, object>> updateHeaders)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return repository.Save(Bucket.Default, aggregate, commitId, updateHeaders);
        }

        public static Task Save([NotNull] this IRepository repository, string bucketId, IAggregate aggregate, Guid commitId)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return repository.Save(bucketId, aggregate, commitId, a => { });
        }
    }
}