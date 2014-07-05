namespace Cedar.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public static class CedarClientExtensions
    {
        public static Task ExecuteCommand(this CedarClient client, string vendor, object command, Guid commandId)
        {
            return ExecuteCommand(client, command, commandId, GetJsonCommandHttpContent(vendor, client.SerializerSettings));
        }

        public static async Task ExecuteCommand(this CedarClient client, object command, Guid commandId, Func<object, HttpContent> createHttpContent)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/commands/{0}".FormatWith(commandId))
            {
                Content = createHttpContent(command),
            };
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = await client.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var exceptionResponse = await response.Content.ReadAs<ExceptionResponse>(client.SerializerSettings);
                throw client.ExceptionFactory.Create(exceptionResponse);
            }
        }

        private static Func<object, HttpContent> GetJsonCommandHttpContent(string vendor, JsonSerializerSettings serializerSettings)
        {
            return command =>
            {
                string commandJson = JsonConvert.SerializeObject(command, serializerSettings);
                var httpContent = new StringContent(commandJson);
                httpContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse("application/vnd.{0}.{1}+json".FormatWith(vendor, command.GetType().Name.ToLower()));
                return httpContent;
            };
        }
    }
}