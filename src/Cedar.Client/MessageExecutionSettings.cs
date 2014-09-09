namespace Cedar
{
    using Cedar.ContentNegotiation.Client;

    public abstract class MessageExecutionSettings : IMessageExecutionSettings
    {
        private readonly string _vendor;
        private readonly string _path;
        private readonly IModelToExceptionConverter _modelToExceptionConverter;
        private readonly ISerializer _serializer;

        protected MessageExecutionSettings(
            string vendor,
            IModelToExceptionConverter modelToExceptionConverter = null,
            ISerializer serializer = null,
            string path = null)
        {
            _vendor = vendor;
            _path = path ?? string.Empty;
            _modelToExceptionConverter = modelToExceptionConverter ?? new ModelToExceptionConverter();
            _serializer = serializer ?? new DefaultJsonSerializer();
        }

        public string Vendor
        {
            get { return _vendor; }
        }

        public IModelToExceptionConverter ModelToExceptionConverter
        {
            get { return _modelToExceptionConverter; }
        }

        public string Path
        {
            get { return _path; }
        }

        public ISerializer Serializer
        {
            get { return _serializer; }
        }
    }
}