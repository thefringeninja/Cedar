namespace CED
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using CED.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class CEDClientExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static Task ExecuteCommand(this CEDClient client, object command, Guid commandId)
        {
            return ExecuteCommand(client, command, commandId, GetJsonCommandHttpContent("cedar"));
        }

        public static async Task ExecuteCommand(this CEDClient client, object command, Guid commandId, Func<object, HttpContent> createHttpContent)
        {
            HttpResponseMessage response = await client.HttpClient.PutAsync("/commands/{0}".FormatWith(commandId), createHttpContent(command));
        }

        public static Func<object, HttpContent> GetJsonCommandHttpContent(string vendor)
        {
            return command =>
            {
                string commandJson = JsonConvert.SerializeObject(command, SerializerSettings);
                var httpContent = new StringContent(commandJson);
                httpContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse("application/vnd.{0}.{1}+json".FormatWith(vendor, command.GetType().Name));
                return httpContent;
            };
        }
    }
}