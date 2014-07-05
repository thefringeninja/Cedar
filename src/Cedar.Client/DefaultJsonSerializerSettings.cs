namespace Cedar.Client
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class DefaultJsonSerializerSettings
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.All
        };
    }
}