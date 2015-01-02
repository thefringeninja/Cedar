namespace Cedar
{
    using Cedar.ExceptionModels.Client;
    using Cedar.Serialization;

    public class MessageExecutionSettings : IMessageExecutionSettings
    {
        private readonly string _path;
        private readonly IModelToExceptionConverter _modelToExceptionConverter;
        private readonly ISerializer _serializer;

        public MessageExecutionSettings(
            IModelToExceptionConverter modelToExceptionConverter = null,
            ISerializer serializer = null,
            string path = null)
        {
            _path = path ?? string.Empty;
            _modelToExceptionConverter = modelToExceptionConverter ?? new ModelToExceptionConverter();
            _serializer = serializer ?? new DefaultJsonSerializer();
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