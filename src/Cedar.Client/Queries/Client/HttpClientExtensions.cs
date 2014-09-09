// ReSharper disable once CheckNamespace
namespace Cedar.Queries.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.Commands.Client;
    using Cedar.ContentNegotiation.Client;
    using Cedar.ExceptionModels.Client;
    using Newtonsoft.Json;
    
    public static class HttpClientExtensions
    {
        public static async Task<TOutput> ExecuteQuery<TInput, TOutput>(this HttpClient client, TInput input, Guid commandId, IMessageExecutionSettings settings)
        {
            string queryJson = JsonConvert.SerializeObject(input, DefaultJsonSerializerSettings.Settings);
            var httpContent = new StringContent(queryJson);
            httpContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/vnd.{0}.{1}+json".FormatWith(settings.Vendor, typeof(TInput).Name).ToLower());

            var request = new HttpRequestMessage(HttpMethod.Get, settings.Path + "/{0}".FormatWith(typeof(TInput).Name).ToLower())
            {
                Content = httpContent
            };
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Got 404 Not Found for {0}".FormatWith(request.RequestUri));
            }
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var exceptionModel = await response.Content.ReadObject(DefaultJsonSerializerSettings.Settings) as ExceptionModel;
                throw settings.ModelToExceptionConverter.Convert(exceptionModel);
            }
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var exceptionModel = await response.Content.ReadObject(DefaultJsonSerializerSettings.Settings) as ExceptionModel;
                throw settings.ModelToExceptionConverter.Convert(exceptionModel);
            }
            return default(TOutput);
        }
    }
}