namespace Cedar.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using NEventStore;

    public static class RepositoryExtensions
    {
        public static Task<TAggregate> GetById<TAggregate>([NotNull] this IRepository repository, Guid id, CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            return GetById<TAggregate>(repository, Bucket.Default, id, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>([NotNull] this IRepository repository, Guid id, int version, CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            return repository.GetById<TAggregate>(Bucket.Default, id, version, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IRepository repository,
            string bucketId, Guid id,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            return repository.GetById<TAggregate>(bucketId, id, int.MaxValue, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>([NotNull] this IRepository repository, string id, CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            return GetById<TAggregate>(repository, Bucket.Default, id, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>([NotNull] this IRepository repository, string id, int version, CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            return repository.GetById<TAggregate>(Bucket.Default, id, version, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IRepository repository,
            string bucketId, string id,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            return repository.GetById<TAggregate>(bucketId, id, int.MaxValue, cancellationToken);
        }

        public static Task Save([NotNull] this IRepository repository, IAggregate aggregate, Guid commitId, CancellationToken cancellationToken)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return repository.Save(aggregate, commitId, a => { }, cancellationToken);
        }

        public static Task Save(
            [NotNull] this IRepository repository,
            IAggregate aggregate,
            Guid commitId,
            Action<IDictionary<string, object>> updateHeaders,
            CancellationToken cancellationToken)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return repository.Save(Bucket.Default, aggregate, commitId, updateHeaders, cancellationToken);
        }

        public static Task Save(
            [NotNull] this IRepository repository,
            string bucketId,
            IAggregate aggregate,
            Guid commitId,
            CancellationToken cancellationToken)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return repository.Save(bucketId, aggregate, commitId, a => { }, cancellationToken);
        }
    }
}