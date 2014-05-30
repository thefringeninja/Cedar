namespace CED.Hosting
{
    using System;

    internal static class ExceptionExtensions
    {
        internal static ExceptionResponse ToExceptionResponse(this Exception ex)
        {
            return new ExceptionResponse
            {
                ExeptionType = ex.GetType().Name,
                Message = ex.Message
            };
        }
    }
}