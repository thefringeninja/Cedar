// ReSharper disable once CheckNamespace
namespace Cedar.Queries.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.ExceptionModels.Client;
    using Cedar.Serialization;

    public static class HttpClientExtensions
    {
        public static async Task<TOutput> ExecuteQuery<TInput, TOutput>(this HttpClient client, TInput input, Guid queryId, IMessageExecutionSettings settings, Func<object, Type, Type, IMessageExecutionSettings, HttpRequestMessage> getRequest = null)
        {
            getRequest = getRequest ?? GetRequestFromBody;

            var request = getRequest(input, typeof(TInput), typeof(TOutput), settings);
            
            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            await response.ThrowOnErrorStatus(request, settings);

            var jsonString = await response.Content.ReadAsStringAsync();

            return (TOutput)settings.Serializer.Deserialize(jsonString, typeof (TOutput));
        }

        public static readonly Func<object, Type, Type, IMessageExecutionSettings, HttpRequestMessage>
            GetRequestFromBody = (input, inputType, outputType, settings) =>
            {
                string queryJson = settings.Serializer.Serialize(input);
                var httpContent = new StringContent(queryJson);
                httpContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse(
                        "application/vnd.{0}.{1}+json".FormatWith(settings.Vendor, inputType.Name).ToLower());

                var request = new HttpRequestMessage(HttpMethod.Get,
                    settings.Path + "/{0}".FormatWith(inputType.Name).ToLower())
                {
                    Content = httpContent,
                };

                request.Headers.Accept.ParseAdd(
                    "application/vnd.{0}.{1}+json".FormatWith(settings.Vendor, outputType.Name).ToLower());

                return request;
            };

        public static readonly Func<object, Type, Type, IMessageExecutionSettings, HttpRequestMessage>
            GetRequestFromQuery = (input, inputType, outputType, settings) =>
            {
                string queryJson = settings.Serializer.Serialize(input);
                var httpContent = new StringContent("");
                httpContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse(
                        string.Format("application/vnd.{0}.{1}+json", settings.Vendor, inputType.Name).ToLower());

                var request = new HttpRequestMessage(HttpMethod.Get,
                    settings.Path + string.Format("/{0}?{1}", inputType.Name, queryJson).ToLower())
                {
                    Content = httpContent
                };

                request.Headers.Accept.ParseAdd(
                    string.Format("application/vnd.{0}.{1}+json", settings.Vendor, outputType.Name).ToLower());

                return request;
            };
    }
}