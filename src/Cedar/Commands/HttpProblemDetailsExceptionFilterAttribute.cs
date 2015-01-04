namespace Cedar.Commands
{
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Filters;

    internal class HttpProblemDetailsExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly CreateProblemDetails _createProblemDetails;

        internal HttpProblemDetailsExceptionFilterAttribute(CreateProblemDetails createProblemDetails)
        {
            _createProblemDetails = createProblemDetails;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var httpProblemDetailsException = actionExecutedContext.Exception as HttpProblemDetailsException;
            HttpProblemDetails problemDetails = httpProblemDetailsException != null 
                ? httpProblemDetailsException.ProblemDetails
                : _createProblemDetails(actionExecutedContext.Exception); // may return null if no mapping from exceptions to problem details has been setup.

            if(problemDetails == null)
            {
                base.OnException(actionExecutedContext);
                return;
            }

            var config = actionExecutedContext.ActionContext.ControllerContext.Configuration;
            var negotiator = config.Services.GetContentNegotiator();
            var formatters = config.Formatters;
            var type = problemDetails.GetType();  // we may be dealing with a type that inherits from HttpProblemDetails because of extensibility

            ContentNegotiationResult result = negotiator.Negotiate(
                type,
                actionExecutedContext.Request,
                formatters);

            if (result == null)
            {
                base.OnException(actionExecutedContext);
                return;
            }

            var response = new HttpResponseMessage(problemDetails.Status)
            {
                Content = new ObjectContent(
                    type,
                    problemDetails,
                    result.Formatter,
                    result.MediaType)
            };
            response.Headers.Add(HttpProblemDetails.HttpProblemDetailsTypeHeader, type.FullName);
            actionExecutedContext.Response = response;
        }
    }
}