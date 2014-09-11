// ReSharper disable once CheckNamespace
namespace Cedar.Queries.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.ExceptionModels.Client;
    using Cedar.Serialization.Client;

    public static class HttpClientExtensions
    {
        public static async Task<TOutput> ExecuteQuery<TInput, TOutput>(this HttpClient client, TInput input, Guid queryId, IMessageExecutionSettings settings)
        {
            string queryJson = settings.Serializer.Serialize(input);
            var httpContent = new StringContent(queryJson);
            httpContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/vnd.{0}.{1}+json".FormatWith(settings.Vendor, typeof(TInput).Name).ToLower());

            var request = new HttpRequestMessage(HttpMethod.Get, settings.Path + "/{0}".FormatWith(typeof(TInput).Name).ToLower())
            {
                Content = httpContent,
            };
            
            request.Headers.Accept.ParseAdd("application/vnd.{0}.{1}+json".FormatWith(settings.Vendor, typeof(TOutput).Name).ToLower());
            
            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            await response.ThrowOnErrorStatus(request, settings);

            var jsonString = await response.Content.ReadAsStringAsync();
            return (TOutput)settings.Serializer.Deserialize(jsonString, typeof (TOutput));
        }
    }
}