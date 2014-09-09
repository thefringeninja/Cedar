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
            return context.HandleException(options, 400, "Bad Request", ex);
        }

        internal static Task HandleInternalServerError(this IOwinContext context, Exception ex, HandlerSettings options)
        {
            return context.HandleException(options, 500, "Internal Server Error", ex);
        }

        internal static Task HandleException(this IOwinContext context, HandlerSettings options, int statusCode, string reasonPhrase, Exception ex, string contentType = "application/json")
        {
            context.Response.StatusCode = statusCode;
            context.Response.ReasonPhrase = reasonPhrase;
            context.Response.ContentType = contentType;
            ExceptionModel exceptionModel = options.ExceptionToModelConverter.Convert(ex);
            string exceptionJson = options.Serialize(exceptionModel);
            byte[] exceptionBytes = Encoding.UTF8.GetBytes(exceptionJson);
            context.Response.ContentLength = exceptionBytes.Length;
            return context.Response.WriteAsync(exceptionBytes);
        }
    }
}
