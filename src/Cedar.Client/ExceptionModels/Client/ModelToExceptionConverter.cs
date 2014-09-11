namespace Cedar.ExceptionModels.Client
{
    using System;

    public class ModelToExceptionConverter : IModelToExceptionConverter
    {
        public virtual Exception Convert(ExceptionModel model)
        {
            Exception exception = null;
            TypeSwitch.On(model)
                .Case<ArgumentNullExceptionModel>(m => exception = new ArgumentNullException(m.ParamName, m.Message))
                .Case<ArgumentExceptionModel>(m => exception = new ArgumentException(m.ParamName, m.Message))
                .Case<InvalidOperationExceptionModel>(m => exception = new InvalidOperationException(m.Message))
                .Case<NotSupportedExceptionModel>(m => exception = new NotSupportedException(m.Message))
                .Default(m => exception = new Exception(m.Message));

            return exception;
        }
    }
}