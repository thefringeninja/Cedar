namespace Cedar
{
    using Cedar.ExceptionModels.Client;
    using Cedar.Serialization;

    public interface IMessageExecutionSettings
    {
        IModelToExceptionConverter ModelToExceptionConverter { get; }

        string Path { get; }

        ISerializer Serializer { get; }
    }
}