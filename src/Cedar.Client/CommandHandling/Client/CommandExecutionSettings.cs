namespace Cedar.CommandHandling.Client
{
    using Newtonsoft.Json;

    public class CommandExecutionSettings : ICommandExecutionSettings
    {
        private readonly string _vendor;
        private readonly string _path;
        private readonly IModelToExceptionConverter _modelToExceptionConverter;

        public CommandExecutionSettings(
            string vendor,
            IModelToExceptionConverter modelToExceptionConverter = null,
            string path = null)
        {
            _vendor = vendor;
            _path = path ?? string.Empty;
            _modelToExceptionConverter = modelToExceptionConverter ?? new ModelToExceptionConverter();
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
    }
}