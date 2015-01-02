namespace Cedar.Serialization
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class DefaultJsonSerializer : ISerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None
        };
        private readonly JsonSerializer _jsonSerializer;

        internal DefaultJsonSerializer()
        {
            _jsonSerializer = JsonSerializer.Create(Settings);
        }

        public object Deserialize(TextReader reader, Type type)
        {
            return _jsonSerializer.Deserialize(reader, type);
        }

        public void Serialize(TextWriter writer, object source)
        {
            _jsonSerializer.Serialize(writer, source);
        }
    }
}