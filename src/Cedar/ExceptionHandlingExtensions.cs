namespace Cedar
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.ContentNegotiation.Client;
    using Cedar.ExceptionModels.Client;
    using Microsoft.Owin;


    internal static partial class ExceptionHandlingExtensions
    {
        internal static Task HandleBadRequest(this IOwinContext context, InvalidOperationException ex, HandlerSettings options)
        {
            var exception = new HttpStatusException(ex.Message, 400, ex);

            return HandleHttpStatusException(context, exception, options);
        }

        internal static Task HandleInternalServerError(this IOwinContext context, Exception ex, HandlerSettings options)
        {
            var exception = new HttpStatusException(ex.Message, 500, ex);

            return HandleHttpStatusException(context, exception, options);
        }

        internal static Task HandleHttpStatusException(this IOwinContext context, HttpStatusException exception, HandlerSettings options, string contentType = "application/json")
        {
            context.Response.StatusCode = (int) exception.StatusCode;
            context.Response.ReasonPhrase = exception.StatusCode.ToString();
            context.Response.ContentType = contentType;
            ExceptionModel exceptionModel = options.ExceptionToModelConverter.Convert(exception.InnerException);
            string exceptionJson = options.Serializer.Serialize(exceptionModel);
            byte[] exceptionBytes = Encoding.UTF8.GetBytes(exceptionJson);
            context.Response.ContentLength = exceptionBytes.Length;
            return context.Response.WriteAsync(exceptionBytes);
        }
    }
}
