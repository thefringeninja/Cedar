namespace Cedar.CommandHandling.Modules
{
    using System;
    using Cedar.CommandHandling.ExceptionHandling;
    using Nancy;
    using Nancy.Responses.Negotiation;

    public class CommandHandlerTests
    {
        public class CustomException : Exception
        {}

        public class CustomExceptionHandler : CommandExceptionHandler<CustomException>
        {
            protected override Negotiator Handle(CustomException exception, Negotiator negotiator)
            {
                return negotiator
                        .WithStatusCode(HttpStatusCode.BadRequest)
                        .WithReasonPhrase("Command validation failed")
                        .WithModel(exception.ToExceptionResponse());
            }
        }
    }
}