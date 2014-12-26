namespace Cedar.ExceptionModels
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.ExceptionModels.Client;
    using Cedar.Serialization;

    // The whole exception handling this will be changed in a seperate refactor, just implementing this to keep tests green
    
    internal static class ExceptionHandlingExtensionsTempWebApi
    {
        internal static async Task<HttpResponseMessage> ExecuteWithExceptionHandling_ThisIsToBeReplaced(
            this Func<Task> actionToRun,
            IExceptionToModelConverter exceptionToModelConverter,
            ISerializer serializer)
        {
            Exception caughtException;
            try
            {
                await actionToRun();
                return null;
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            var aggregateException = caughtException as AggregateException;
            if(aggregateException != null)
            {
                caughtException = aggregateException.InnerExceptions.First();
            }

            var httpStatusException = caughtException as HttpStatusException;
            if (httpStatusException != null)
            {
                return HandleHttpStatusException(httpStatusException, exceptionToModelConverter, serializer);
            }
            var invalidOperationException = caughtException as InvalidOperationException;
            if (invalidOperationException != null)
            {
                return HandleBadRequest(invalidOperationException, exceptionToModelConverter, serializer);
            }
            var argumentException = caughtException as ArgumentException;
            if (argumentException != null)
            {
                return HandleBadRequest(argumentException, exceptionToModelConverter, serializer);
            }
            var formatException = caughtException as FormatException;
            if (formatException != null)
            {
                return HandleBadRequest(formatException, exceptionToModelConverter, serializer);
            }
            var securityException = caughtException as SecurityException;
            if (securityException != null)
            {
                return HandleBadRequest(securityException, exceptionToModelConverter, serializer);
            }
            return HandleInternalServerError(caughtException, exceptionToModelConverter, serializer);
        }

        private static HttpResponseMessage HandleBadRequest(InvalidOperationException ex, IExceptionToModelConverter exceptionToModelConverter,
            ISerializer serializer)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.BadRequest, ex);

            return HandleHttpStatusException(exception, exceptionToModelConverter, serializer);
        }

        private static HttpResponseMessage HandleBadRequest(ArgumentException ex, IExceptionToModelConverter exceptionToModelConverter,
            ISerializer serializer)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.BadRequest, ex);

            return HandleHttpStatusException(exception, exceptionToModelConverter, serializer);
        }

        private static HttpResponseMessage HandleBadRequest(FormatException ex, IExceptionToModelConverter exceptionToModelConverter,
            ISerializer serializer)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.BadRequest, ex);

            return HandleHttpStatusException(exception, exceptionToModelConverter, serializer);
        }

        private static HttpResponseMessage HandleBadRequest(SecurityException ex, IExceptionToModelConverter exceptionToModelConverter,
            ISerializer serializer)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.Forbidden, ex);

            return HandleHttpStatusException(exception, exceptionToModelConverter, serializer);
        }

        private static HttpResponseMessage HandleInternalServerError(Exception ex, IExceptionToModelConverter exceptionToModelConverter,
            ISerializer serializer)
        {
            var exception = new HttpStatusException(ex.Message, HttpStatusCode.InternalServerError, ex);

            return HandleHttpStatusException(exception, exceptionToModelConverter, serializer);
        }

        private static HttpResponseMessage HandleHttpStatusException(
            HttpStatusException exception,
            IExceptionToModelConverter exceptionToModelConverter,
            ISerializer serializer,
            string contentType = "application/json")
        {
            var response = new HttpResponseMessage(exception.StatusCode);
            ExceptionModel exceptionModel = exceptionToModelConverter.Convert(exception);
            string exceptionJson = serializer.Serialize(exceptionModel);
            response.Content = new StringContent(exceptionJson, Encoding.UTF8, contentType);
            return response;
        }
    }
}
