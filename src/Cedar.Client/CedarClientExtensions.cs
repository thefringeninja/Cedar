namespace Cedar.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public static class CedarClientExtensions
    {
        public static Task ExecuteCommand(this CedarClient client, object command, Guid commandId)
        {
            return ExecuteCommand(client, command, commandId, GetJsonCommandHttpContent("cedar", client.SerializerSettings));
        }

        public static async Task ExecuteCommand(this CedarClient client, object command, Guid commandId, Func<object, HttpContent> createHttpContent)
        {
            HttpResponseMessage response = await client.HttpClient.PutAsync("/commands/{0}".FormatWith(commandId), createHttpContent(command));
        }

        public static Func<object, HttpContent> GetJsonCommandHttpContent(string vendor, JsonSerializerSettings serializerSettings)
        {
            return command =>
            {
                string commandJson = JsonConvert.SerializeObject(command, serializerSettings);
                var httpContent = new StringContent(commandJson);
                httpContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse("application/vnd.{0}.{1}+json".FormatWith(vendor, command.GetType().Name));
                return httpContent;
            };
        }
    }
}