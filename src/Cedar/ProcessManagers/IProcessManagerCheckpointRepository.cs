namespace Cedar.ProcessManagers
{
    using System;
    using System.Threading.Tasks;
    using Cedar.ProcessManagers.Messages;

    public interface IProcessManagerCheckpointRepository<T> where T: IComparable<string>
    {
        Task SaveCheckpointToken(IProcessManager processManager, string checkpointToken);
        Task MarkProcessCompleted(ProcessCompleted message);
        Task<T> GetCheckpoint(string processId);
    }
}