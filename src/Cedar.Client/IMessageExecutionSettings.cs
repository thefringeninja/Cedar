namespace Cedar
{
    using Cedar.ContentNegotiation.Client;

    public interface IMessageExecutionSettings
    {
        string Vendor { get; }

        IModelToExceptionConverter ModelToExceptionConverter { get; }

        string Path { get; }
    }
}