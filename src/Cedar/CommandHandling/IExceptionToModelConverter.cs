namespace Cedar.CommandHandling
{
    using System;
    using Cedar.CommandHandling.ExceptionModels;

    public interface IExceptionToModelConverter
    {
        ExceptionModel Convert(Exception exception);
    }
}