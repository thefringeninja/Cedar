namespace Cedar.ContentNegotiation.Client
{
    using System;
    using Cedar.ExceptionModels.Client;

    public interface IModelToExceptionConverter
    {
        Exception Convert(ExceptionModel model);
    }
}