namespace Cedar.ContentNegotiation
{
    using System;
    using Cedar.ExceptionModels.Client;

    public interface IExceptionToModelConverter
    {
        ExceptionModel Convert(Exception exception);
    }
}