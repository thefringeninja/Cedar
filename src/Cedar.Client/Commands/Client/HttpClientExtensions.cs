// ReSharper disable once CheckNamespace
namespace Cedar.Commands.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.ContentNegotiation.Client;
    using Cedar.ExceptionModels.Client;

    public static class HttpClientExtensions
    {
        public static async Task ExecuteCommand(this HttpClient client, object command, Guid commandId, IMessageExecutionSettings settings)
        {
            string commandJson = settings.Serializer.Serialize(command);
            var httpContent = new StringContent(commandJson);
            httpContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/vnd.{0}.{1}+json".FormatWith(settings.Vendor, command.GetType().Name.ToLower()));

            var request = new HttpRequestMessage(HttpMethod.Put, settings.Path + "/{0}".FormatWith(commandId))
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
                var exception = await settings.Serializer.ReadException(response.Content, settings.ModelToExceptionConverter);
                throw exception;
            }
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var exception = await settings.Serializer.ReadException(response.Content, settings.ModelToExceptionConverter);
                throw exception;
            }
        }
    }
}