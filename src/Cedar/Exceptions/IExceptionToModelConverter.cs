namespace Cedar.Exceptions
{
    using System;
    using Cedar.Client.ExceptionModels;

    public interface IExceptionToModelConverter
    {
        ExceptionModel Convert(Exception exception);
    }
}