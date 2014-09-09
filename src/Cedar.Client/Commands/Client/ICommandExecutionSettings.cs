namespace Cedar.Commands.Client
{
    using Cedar.ContentNegotiation.Client;

    public interface ICommandExecutionSettings
    {
        string Vendor { get; }

        IModelToExceptionConverter ModelToExceptionConverter { get; }

        string Path { get; }
    }
}