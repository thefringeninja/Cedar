namespace Cedar.Domain.Persistence
{
    using System;
    using System.Threading.Tasks;

    public static class RepositoryExtensions
    {
        public static Task Save(this IRepository repository, IAggregate aggregate, Guid commitId)
        {
            return repository.Save(aggregate, commitId, a => { });
        }

        public static Task Save(this IRepository repository, string bucketId, IAggregate aggregate, Guid commitId)
        {
            return repository.Save(bucketId, aggregate, commitId, a => { });
        }
    }
}