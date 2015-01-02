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
        private const string MediaTypeTemplate = "application/vnd.{0}+json";

        public static async Task<TOutput> ExecuteQuery<TInput, TOutput>(this HttpClient client, TInput input, Guid queryId, IMessageExecutionSettings settings, Func<object, Type, Type, IMessageExecutionSettings, HttpRequestMessage> getRequest = null)
        {
            getRequest = getRequest ?? GetRequestFromBody;

            var request = getRequest(input, typeof(TInput), typeof(TOutput), settings);
            
            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            await response.ThrowOnErrorStatus(request, settings);

            var jsonString = await response.Content.ReadAsStringAsync();

            return settings.Serializer.Deserialize<TOutput>(jsonString);
        }

        public static readonly Func<object, Type, Type, IMessageExecutionSettings, HttpRequestMessage>
            GetRequestFromBody = (input, inputType, outputType, settings) =>
            {
                string queryJson = settings.Serializer.Serialize(input);
                var httpContent = new StringContent(queryJson);
                httpContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse(MediaTypeTemplate.FormatWith(inputType.Name).ToLower());

                var request = new HttpRequestMessage(HttpMethod.Get,
                    settings.Path + "/{0}".FormatWith(inputType.Name).ToLower())
                {
                    Content = httpContent,
                };

                request.Headers.Accept.ParseAdd(MediaTypeTemplate.FormatWith(outputType.Name).ToLower());

                return request;
            };

        public static readonly Func<object, Type, Type, IMessageExecutionSettings, HttpRequestMessage>
            GetRequestFromQuery = (input, inputType, outputType, settings) =>
            {
                string queryJson = settings.Serializer.Serialize(input);
                var httpContent = new StringContent("");
                httpContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse(
                        string.Format(MediaTypeTemplate, inputType.Name).ToLower());

                var request = new HttpRequestMessage(HttpMethod.Get,
                    settings.Path + string.Format("/{0}?{1}", inputType.Name, queryJson).ToLower())
                {
                    Content = httpContent
                };

                request.Headers.Accept.ParseAdd(MediaTypeTemplate.FormatWith(outputType.Name));

                return request;
            };
    }
}