namespace Cedar.Client.ExceptionModels
{
    public class ExceptionModel
    {
        public string Type { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public ExceptionModel InnerException { get; set; }
    }
}