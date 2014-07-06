namespace Cedar.CommandHandling
{
    using System;
    using Cedar.Client;
    using Cedar.Exceptions;

    public class ExceptionToModelConverter : IExceptionToModelConverter
    {
        public virtual ExceptionModel Convert(Exception exception)
        {
            ExceptionModel model = null;

            TypeSwitch.On(exception)
                .Case<ArgumentException>(ex => model = new ArgumentExceptionModel
                {
                    ParamName = ex.ParamName,
                })
                .Case<NotSupportedException>(ex => model = new NotSupportedExceptionModel())
                .Default(() => model = new ExceptionModel
                {
                    Message = string.Format("[No exception serializer found for {0}].{1}", exception.GetType(), Environment.NewLine)
                });

            model.Message = exception.Message;
            model.StackTrace = exception.StackTrace;
            return model;
        }
    }
}