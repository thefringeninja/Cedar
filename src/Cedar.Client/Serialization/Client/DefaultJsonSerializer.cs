namespace Cedar.Serialization.Client
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    internal class DefaultJsonSerializer : ISerializer
    {
        private readonly JsonSerializer _jsonSerializer;

        internal DefaultJsonSerializer()
        {
            _jsonSerializer = JsonSerializer.Create(DefaultJsonSerializerSettings.Settings);
        }

        public object Deserialize(TextReader reader, Type type)
        {
            return _jsonSerializer.Deserialize(reader, type);
        }

        public void Serialize(TextWriter writer, object target)
        {
            _jsonSerializer.Serialize(writer, target);
        }
    }
}