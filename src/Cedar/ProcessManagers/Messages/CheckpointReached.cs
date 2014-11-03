namespace Cedar.ProcessManagers.Messages
{
    public class CheckpointReached
    {
        public string ProcessId { get; set; }
        public string CorrelationId { get; set; }
        public string CheckpointToken { get; set; }
    }
}