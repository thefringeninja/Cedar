namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProcessManagerRepository
    {
        Task<TProcess> GetById<TProcess>(string bucketId, string id, int versionToLoad, CancellationToken token)
            where TProcess : IProcessManager;

        Task Save<TProcess>(string bucketId, TProcess process, Guid commitId,
            Action<IDictionary<string, object>> updateHeaders, CancellationToken token) where TProcess : IProcessManager;
    }
}