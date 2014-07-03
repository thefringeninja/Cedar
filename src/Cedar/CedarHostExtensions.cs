namespace Cedar
{
    using System;
    using System.Net.Http;
    using Cedar.Client;

    public static class CedarHostExtensions
    {
        public static CedarClient CreateClient(this CedarHost host)
        {
            return new CedarClient(new Uri("http://localhost"), new OwinHttpMessageHandler(host.AppFunc));
        }
    }
}