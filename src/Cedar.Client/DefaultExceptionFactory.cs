namespace Cedar.Client
{
    using System;

    internal class DefaultExceptionFactory : IExceptionFactory
    {
        public Exception Create(ExceptionResponse exceptionResponse)
        {
            throw new InvalidOperationException(exceptionResponse.Message);
        }
    }
}