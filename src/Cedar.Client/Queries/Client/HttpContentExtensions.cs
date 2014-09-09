// ReSharper disable once CheckNamespace
namespace Cedar.Queries.Client
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    internal static class HttpContentExtensions
    {
        internal static async Task<TOutput> ReadObject<TOutput>(this HttpContent content, JsonSerializerSettings serializerSettings)
        {
            var jsonString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TOutput>(jsonString, serializerSettings);
        }
    }
}