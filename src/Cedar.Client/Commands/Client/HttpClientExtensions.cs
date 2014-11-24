// ReSharper disable once CheckNamespace
namespace Cedar.Commands.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.ExceptionModels.Client;
    using Cedar.Serialization;

    public static class HttpClientExtensions
    {
        public static Task<CommandResult> ExecuteCommand(this HttpClient client, object command, Guid commandId, IMessageExecutionSettings settings)
        {
            var request = CreatePutRequest(command, commandId, settings);

            return client.SendRequest(request, settings);
        }

        public static Task<CommandResult> GetCommandStatus(this HttpClient client, Guid commandId, IMessageExecutionSettings settings)
        {
            return client.SendRequest(new HttpRequestMessage(HttpMethod.Get, settings.Path + "/{0}".FormatWith(commandId)), settings);
        }

        private static HttpRequestMessage CreatePutRequest(object command, Guid commandId, IMessageExecutionSettings settings)
        {
            string commandJson = settings.Serializer.Serialize(command);
            var httpContent = new StringContent(commandJson);
            httpContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/vnd.{0}.{1}+json".FormatWith(settings.Vendor,
                    command.GetType().Name.ToLower()));

            var request = new HttpRequestMessage(HttpMethod.Put, settings.Path + "/{0}".FormatWith(commandId))
            {
                Content = httpContent
            };
            return request;
        }

        private static async Task<CommandResult> SendRequest(this HttpClient client, HttpRequestMessage request, IMessageExecutionSettings settings)
        {
            request.Headers.Accept.ParseAdd("application/json");

            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            await response.ThrowOnErrorStatus(request, settings);

            return (CommandResult)settings.Serializer.Deserialize(
                await response.Content.ReadAsStringAsync(),
                typeof(CommandResult));
        }



    }
}