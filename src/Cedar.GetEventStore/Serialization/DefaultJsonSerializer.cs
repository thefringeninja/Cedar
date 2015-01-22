namespace Cedar.GetEventStore.Serialization
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class DefaultJsonSerializer : ISerializer
    {
        internal static readonly ISerializer Instance;
        internal static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None
        };

        private readonly JsonSerializer _jsonSerializer;

        static DefaultJsonSerializer()
        {
            Instance = new DefaultJsonSerializer();
        }

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