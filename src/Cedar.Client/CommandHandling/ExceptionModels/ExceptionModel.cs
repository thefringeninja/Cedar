namespace Cedar.CommandHandling.ExceptionModels
{
    public class ExceptionModel
    {
        public string Message { get; set; }

        public string StackTrace { get; set; }

        public ExceptionModel InnerException { get; set; }
    }
}