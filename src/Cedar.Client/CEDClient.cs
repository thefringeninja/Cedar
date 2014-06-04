namespace Cedar.Client
{
    using System;
    using System.Net;
    using System.Net.Http;

    public class CEDClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public CEDClient(Uri baseAddress, HttpMessageHandler handler = null)
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