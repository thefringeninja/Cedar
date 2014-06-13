namespace Cedar.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;

    public class CedarClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _serializerSettings;

        public CedarClient(
            Uri baseAddress,
            HttpMessageHandler handler = null,
            JsonSerializerSettings serializerSettings = null)
        {
            if (handler == null)
            {
                handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };
            }
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = baseAddress
            };
            _serializerSettings = serializerSettings ?? DefaultJsonSerializerSettings.Settings;
        }

        public JsonSerializerSettings SerializerSettings
        {
            get { return _serializerSettings; }
        }

        public HttpClient HttpClient
        {
            get { return _httpClient; }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}