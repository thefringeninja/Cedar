namespace Cedar.Projections
{
    using System.Threading.Tasks;
    using Cedar.Projections.Storage;

    public class InMemoryCheckpointRepository : ICheckpointRepository
    {
        private string _checkpointToken;

        public InMemoryCheckpointRepository(string checkpointToken = null)
        {
            _checkpointToken = checkpointToken;
        }

        public Task<string> Get()
        {
            return Task.FromResult(_checkpointToken);
        }

        public Task Put(string checkpointToken)
        {
            _checkpointToken = checkpointToken;
            return Task.FromResult(0);
        }
    }
}