namespace Cedar.CommandHandling.Serialization
{
    using System;
    using Cedar.Client.ExceptionModels;
    using Cedar.Common;

    public class DefaultExceptionToModelConverter : IExceptionToModelConverter
    {
        public virtual object Convert(Exception exception)
        {
            ExceptionModel exceptionModel = null;

            TypeSwitch.On(exception)
                .Case<ArgumentException>(ex => exceptionModel = new ArgumentExceptionModel
                {
                    ParamName = ex.ParamName,
                })
                .Case<NotSupportedException>(ex => exceptionModel = new NotSupportedExceptionModel())
                .Default(() => exceptionModel = new ExceptionModel
                {
                    Message = string.Format("[No exception serializer found for {0}].{1}", exception.GetType(), Environment.NewLine)
                });

            exceptionModel.Type = exception.GetType().Name;
            exceptionModel.Message = exception.Message;
            exceptionModel.StackTrace = exception.StackTrace;
            return exceptionModel;
        }
    }
}