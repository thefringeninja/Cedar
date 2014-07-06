namespace Cedar.Client
{
    using System;
    using Cedar.Client.ExceptionModels;

    public interface IModelToExceptionConverter
    {
        Exception Convert(ExceptionModel model);
    }
}