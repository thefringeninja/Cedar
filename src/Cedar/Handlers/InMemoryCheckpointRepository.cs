namespace Cedar.Handlers
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an in-memory <see cref="ICheckpointRepository"/>.
    /// </summary>
    public class InMemoryCheckpointRepository : ICheckpointRepository
    {
        private string _checkpointToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCheckpointRepository"/> class.
        /// </summary>
        /// <param name="checkpointToken">The checkpoint token.</param>
        public InMemoryCheckpointRepository(string checkpointToken = null)
        {
            _checkpointToken = checkpointToken;
        }

        /// <summary>
        /// Gets the current checkpoint.
        /// </summary>
        /// <returns>
        /// A <see cref="Task" /> that represents the operation.
        /// </returns>
        public Task<string> Get()
        {
            return Task.FromResult(_checkpointToken);
        }

        /// <summary>
        /// Puts the specified checkpoint token.
        /// </summary>
        /// <param name="checkpointToken">The checkpoint token.</param>
        /// <returns>
        /// A <see cref="Task" /> that represents the operation.
        /// </returns>
        public Task Put(string checkpointToken)
        {
            _checkpointToken = checkpointToken;
            return Task.FromResult(0);
        }
    }
}