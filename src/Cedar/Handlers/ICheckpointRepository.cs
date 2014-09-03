namespace Cedar.Handlers
{
    using System.Threading.Tasks;

    public interface ICheckpointRepository
    {
        Task<string> Get();

        Task Put(string checkpointToken);
    }
}