namespace Cedar.Commands
{
    using System;
    using Cedar.Commands.ExceptionModels;

    public interface IExceptionToModelConverter
    {
        ExceptionModel Convert(Exception exception);
    }
}