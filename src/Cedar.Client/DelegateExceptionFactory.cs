namespace Cedar.Client
{
    using System;

    public class DelegateExceptionFactory : IExceptionFactory
    {
        private readonly Func<ExceptionResponse, Exception> _exceptionFactory;

        public DelegateExceptionFactory(Func<ExceptionResponse, Exception> exceptionFactory)
        {
            _exceptionFactory = exceptionFactory;
        }

        public Exception Create(ExceptionResponse exceptionResponse)
        {
            return _exceptionFactory(exceptionResponse);
        }
    }
}