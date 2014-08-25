namespace Cedar.CommandHandling.Client
{
    using System;
    using Cedar.CommandHandling.ExceptionModels;

    public interface IModelToExceptionConverter
    {
        Exception Convert(ExceptionModel model);
    }
}