namespace Cedar.Handlers
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents durable storage to save and retrieve a checkpoint token.
    /// </summary>
    public interface ICheckpointRepository
    {
        /// <summary>
        /// Gets the current checkpoint.
        /// </summary>
        /// <returns></returns>
        Task<string> Get();

        Task Put(string checkpointToken);
    }
}