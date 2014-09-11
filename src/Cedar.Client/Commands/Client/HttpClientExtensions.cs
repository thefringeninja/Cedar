// ReSharper disable once CheckNamespace
namespace Cedar.Commands.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.ExceptionModels.Client;
    using Cedar.Serialization.Client;

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
            
            await response.ThrowOnErrorStatus(request, settings);
        }
    }
}