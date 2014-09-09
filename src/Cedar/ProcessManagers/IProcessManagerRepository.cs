namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IProcessManagerRepository : IDisposable
    {
        Task<TProcess> GetById<TProcess>(string id, string bucketId = null);
        Task Save<TProcess>(TProcess process, Guid commitId, Action<IDictionary<string, object>> updateHeaders = null, string bucketId = null);
    }
}