namespace Cedar
{
    using System;
    using Cedar.Client;
    using Owin;

    public static class CedarHostExtensions
    {
        public static CedarClient CreateClient(this CedarHost host, IModelToExceptionConverter modelToExceptionConverter = null)
        {
            return new CedarClient(
                new Uri("http://localhost"),
                new OwinHttpMessageHandler(host.AppFunc),
                modelToExceptionConverter: modelToExceptionConverter);
        }
    }
}