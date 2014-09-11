namespace Cedar.ExceptionModels
{
    using System;
    using Cedar.ExceptionModels.Client;

    public interface IExceptionToModelConverter
    {
        ExceptionModel Convert(Exception exception);
    }
}