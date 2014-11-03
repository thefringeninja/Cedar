namespace Cedar.ProcessManagers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.ProcessManagers.Messages;

    public interface IProcessManagerCheckpointRepository<T> where T: IComparable<string>
    {
        Task SaveCheckpointToken(IProcessManager processManager, string checkpointToken, CancellationToken ct);
        Task MarkProcessCompleted(ProcessCompleted message, CancellationToken ct);
        Task<T> GetCheckpoint(string processId, CancellationToken ct);
    }
}