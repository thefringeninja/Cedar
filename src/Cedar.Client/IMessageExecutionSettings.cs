namespace Cedar
{
    using Cedar.ExceptionModels.Client;
    using Cedar.Serialization.Client;

    public interface IMessageExecutionSettings
    {
        string Vendor { get; }

        IModelToExceptionConverter ModelToExceptionConverter { get; }

        string Path { get; }

        ISerializer Serializer { get; }
    }
}