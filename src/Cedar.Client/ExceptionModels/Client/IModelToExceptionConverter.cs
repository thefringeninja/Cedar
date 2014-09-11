namespace Cedar.ExceptionModels.Client
{
    using System;

    public interface IModelToExceptionConverter
    {
        Exception Convert(ExceptionModel model);
    }
}