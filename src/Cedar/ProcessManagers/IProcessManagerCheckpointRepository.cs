namespace Cedar.ProcessManagers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.ProcessManagers.Messages;

    public interface IProcessManagerCheckpointRepository<T> where T: IComparable<string>
    {
        Task SaveCheckpointToken(IProcessManager processManager, string checkpointToken, CancellationToken ct, string bucketId = null);
        Task MarkProcessCompleted(ProcessCompleted message, CancellationToken ct, string bucketId = null);
        Task<T> GetCheckpoint(string processId, CancellationToken ct, string bucketId = null);
    }
}