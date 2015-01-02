namespace Cedar.Queries.Client
{
    using Cedar.ExceptionModels.Client;
    using Cedar.Serialization;

    public class QueryExecutionSettings : MessageExecutionSettings
    {
        public QueryExecutionSettings(
            IModelToExceptionConverter modelToExceptionConverter = null,
            ISerializer serializer = null,
            string path = "query")
            : base(modelToExceptionConverter, serializer, path)
        { }
    }
}
