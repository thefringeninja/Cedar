namespace Cedar.ContentNegotiation.Client
{
    using System.Net.Http;
    using System.Threading.Tasks;

    internal static class HttpContentExtensions
    {
        internal static async Task<T> ReadObject<T>(this ISerializer serializer, HttpContent content)
        {
            var jsonString = await content.ReadAsStringAsync();
            return (T)serializer.Deserialize(jsonString, typeof(T));
        }
    }
}