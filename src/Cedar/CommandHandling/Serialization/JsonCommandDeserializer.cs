namespace Cedar.CommandHandling.Serialization
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Cedar.Hosting;
    using Newtonsoft.Json;

    public class JsonCommandDeserializer : ICommandDeserializer
    {
        private readonly JsonSerializerSettings _jsonSettings;

        public JsonCommandDeserializer(JsonSerializerSettings jsonSettings = null)
        {
            _jsonSettings = jsonSettings ?? DefaultJsonSerializerSettings.Settings;
        }

        public bool Handles(string contentType)
        {
            return contentType.Equals(@"application/json", StringComparison.OrdinalIgnoreCase)
                   || contentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<object> Deserialize(Stream stream, Type commandType)
        {
            string json;
            using (var streamReader = new StreamReader(stream))
            {
                json = await streamReader.ReadToEndAsync();
            }
            return JsonConvert.DeserializeObject(json, commandType, _jsonSettings);
        }
    }
}