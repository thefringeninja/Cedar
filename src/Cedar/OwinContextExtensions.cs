namespace Cedar
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Commands.ExceptionModels;
    using Cedar.ContentNegotiation;
    using Microsoft.Owin;

    internal static class OwinContextExtensions
    {
        internal static Task HandleBadRequest(this IOwinContext context, InvalidOperationException ex, HandlerSettings options)
        {
            context.Response.StatusCode = 400;
            context.Response.ReasonPhrase = "Bad Request";
            context.Response.ContentType = "application/json";
            ExceptionModel exceptionModel = options.ExceptionToModelConverter.Convert(ex);
            string exceptionJson = options.Serialize(exceptionModel);
            byte[] exceptionBytes = Encoding.UTF8.GetBytes(exceptionJson);
            context.Response.ContentLength = exceptionBytes.Length;
            return context.Response.WriteAsync(exceptionBytes);
        }

        internal static Task HandleInternalServerError(this IOwinContext context, Exception ex, HandlerSettings options)
        {
            context.Response.StatusCode = 500;
            context.Response.ReasonPhrase = "Internal Server Error";
            context.Response.ContentType = "application/json";
            ExceptionModel exceptionModel = options.ExceptionToModelConverter.Convert(ex);
            string exceptionJson = options.Serialize(exceptionModel);
            byte[] exceptionBytes = Encoding.UTF8.GetBytes(exceptionJson);
            context.Response.ContentLength = exceptionBytes.Length;
            return context.Response.WriteAsync(exceptionBytes);
        }
    }
}
