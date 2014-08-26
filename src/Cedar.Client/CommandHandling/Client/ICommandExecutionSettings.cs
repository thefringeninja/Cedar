namespace Cedar.CommandHandling.Client
{
    using Newtonsoft.Json;

    public interface ICommandExecutionSettings
    {
        string Vendor { get; }

        IModelToExceptionConverter ModelToExceptionConverter { get; }

        string Path { get; }
    }
}