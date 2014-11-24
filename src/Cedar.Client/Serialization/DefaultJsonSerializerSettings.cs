namespace Cedar.Serialization
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal static class DefaultJsonSerializerSettings
    {
        internal static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None
        };
    }
}