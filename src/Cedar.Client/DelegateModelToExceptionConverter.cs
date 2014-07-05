namespace Cedar.Client
{
    using System;
    using Cedar.Client.ExceptionModels;

    public class DelegateModelToExceptionConverter : IModelToExceptionConverter
    {
        private readonly Func<ExceptionModel, Exception> _exceptionFactory;

        public DelegateModelToExceptionConverter(Func<ExceptionModel, Exception> exceptionFactory)
        {
            _exceptionFactory = exceptionFactory;
        }

        public Exception Convert(ExceptionModel exceptionModel)
        {
            return _exceptionFactory(exceptionModel);
        }
    }
}