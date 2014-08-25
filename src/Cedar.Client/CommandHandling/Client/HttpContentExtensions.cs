// ReSharper disable once CheckNamespace
namespace System.Net.Http
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    internal static class HttpContentExtensions
    {
        internal static async Task<object> ReadObject(this HttpContent content, JsonSerializerSettings serializerSettings)
        {
            var jsonString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(jsonString, serializerSettings);
        }
    }
}