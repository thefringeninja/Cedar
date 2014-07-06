namespace Cedar.Client
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    internal static class HttpContentExtensions
    {
        internal static async Task<object> ReadObject(this HttpContent content, JsonSerializerSettings serializerSettings)
        {
            if(content.Headers.ContentType.MediaType != "application/json")
            {
                return null;
            }
            var jsonString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(jsonString, serializerSettings);
        }
    }
}