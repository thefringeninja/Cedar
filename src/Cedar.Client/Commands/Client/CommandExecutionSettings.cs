namespace Cedar.Commands.Client
{
    using Cedar.ContentNegotiation.Client;

    public class CommandExecutionSettings : MessageExecutionSettings
    {
        public CommandExecutionSettings(
            string vendor, 
            IModelToExceptionConverter modelToExceptionConverter = null,
            ISerializer serializer = null,
            string path = "commands") 
            : base(vendor, modelToExceptionConverter, serializer, path)
        {}
    }
}
