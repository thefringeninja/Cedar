namespace Cedar.Client
{
    using System;
    using Cedar.Client.ExceptionModels;
    using Cedar.Common;

    public class DefaultModelToExceptionConverter : IModelToExceptionConverter
    {
        public virtual Exception Convert(ExceptionModel exceptionModel)
        {
            Exception exception = null;
            TypeSwitch.On(exceptionModel)
                .Case<ArgumentNullExceptionModel>(m => exception = new ArgumentNullException(m.ParamName, m.Message))
                .Case<NotSupportedExceptionModel>(m => exception = new NotSupportedException(m.Message))
                .Default(m => exception = new Exception(m.Message));

            return exception;
        }
    }
}