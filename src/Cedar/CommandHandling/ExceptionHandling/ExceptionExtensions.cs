namespace Cedar.CommandHandling.ExceptionHandling
{
    using System;
    using Cedar.Client;

    public static class ExceptionExtensions
    {
        public static ExceptionResponse ToExceptionResponse(this Exception ex)
        {
            return new ExceptionResponse
            {
                ExeptionType = ex.GetType().Name,
                Message = ex.Message
            };
        }
    }
}