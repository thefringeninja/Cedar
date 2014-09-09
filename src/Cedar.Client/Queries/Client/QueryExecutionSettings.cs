namespace Cedar.Queries.Client
{
    using Cedar.ContentNegotiation.Client;

    public class QueryExecutionSettings : MessageExecutionSettings
    {
        public QueryExecutionSettings(
            string vendor,
            IModelToExceptionConverter modelToExceptionConverter = null,
            ISerializer serializer = null,
            string path = "query")
            : base(vendor, modelToExceptionConverter, serializer, path)
        { }
    }
}
