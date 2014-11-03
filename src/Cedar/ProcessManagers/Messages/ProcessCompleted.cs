namespace Cedar.ProcessManagers.Messages
{
    public class ProcessCompleted
    {
        public string ProcessId { get; set; }
        public string CorrelationId { get; set; }

        public override string ToString()
        {
            return string.Format("Process {0} completed.", ProcessId);
        }
    }
}