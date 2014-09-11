namespace Cedar.ExceptionModels.Client
{
    public class ExceptionModel
    {
        private string _typeName;

        public string TypeName
        {
            get { return _typeName ?? (_typeName = GetType().FullName); }
            set { _typeName = value; }
        }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public ExceptionModel InnerException { get; set; }
    }
}