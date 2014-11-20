namespace Cedar.ExceptionModels
{
    using System;
    using System.Net;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.ExceptionModels.Client;
    using Cedar.Owin;
    using Cedar.Serialization.Client;

    internal static class ExceptionHandlingExtensions
    {
        internal static async Task ExecuteWithExceptionHandling(
            this Func<IOwinContext, HandlerSettings, Task> actionToRun,
            IOwinContext context, HandlerSettings options)
        {
            Exception caughtException;
            try
            {
                await actionToRun(context, options);
                return;
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            var httpStatusException = caughtException as HttpStatusException;
            if (httpStatusException != null)
            {
                await context.HandleHttpStatusException(httpStatusException, options);
                return;
            }
            var invalidOperationException = caughtException as InvalidOperationException;
            if (invalidOperationException != null)
            {
                await context.HandleBadRequest(invalidOperationException, options);
                return;
            }
            var argumentException = caughtException as ArgumentException;
            if (argumentException != null)
            {
                await context.HandleBadRequest(argumentException, options);
                return;
            }
            var formatException = caughtException as FormatException;
            if (formatException != null)
            {
                await context.HandleBadRequest(formatException, options);
                return;
            }
            var SecurityException = caughtException as SecurityException;
            if (SecurityException != null)
            {
                await context.HandleBadRequest(SecurityException, options);
                return;
            }
            await context.HandleInternalServerError(caughtException, options);

        }

        private static Task HandleBadRequest(this IOwinContext context, InvalidOperationException ex, HandlerSettings options)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.BadRequest, ex);

            return HandleHttpStatusException(context, exception, options);
        }

        private static Task HandleBadRequest(this IOwinContext context, ArgumentException ex, HandlerSettings options)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.BadRequest, ex);

            return HandleHttpStatusException(context, exception, options);
        }

        private static Task HandleBadRequest(this IOwinContext context, FormatException ex, HandlerSettings options)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.BadRequest, ex);

            return HandleHttpStatusException(context, exception, options);
        }

        private static Task HandleBadRequest(this IOwinContext context, SecurityException ex, HandlerSettings options)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.Forbidden, ex);

            return HandleHttpStatusException(context, exception, options);
        }

        private static Task HandleInternalServerError(this IOwinContext context, Exception ex, HandlerSettings options)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.InternalServerError, ex);

            return HandleHttpStatusException(context, exception, options);
        }

        private static Task HandleHttpStatusException(this IOwinContext context, HttpStatusException exception, HandlerSettings options, string contentType = "application/json")
        {
            context.Response.StatusCode = (int) exception.StatusCode;
            context.Response.ReasonPhrase = exception.StatusCode.ToString();
            context.Response.ContentType = contentType;
            ExceptionModel exceptionModel = options.ExceptionToModelConverter.Convert(exception);
            string exceptionJson = options.Serializer.Serialize(exceptionModel);
            byte[] exceptionBytes = Encoding.UTF8.GetBytes(exceptionJson);
            context.Response.ContentLength = exceptionBytes.Length;
            return context.Response.WriteAsync(exceptionBytes);
        }
    }
}
