namespace Cedar.Client
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    internal static class HttpContentExtensions
    {
        internal static async Task<T> ReadAs<T>(this HttpContent content, JsonSerializerSettings serializerSettings)
        {
            if(content.Headers.ContentType.MediaType != "application/json")
            {
                return default(T);
            }
            var jsonString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonString, serializerSettings);
        }
    }
}