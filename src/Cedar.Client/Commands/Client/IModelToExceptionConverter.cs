namespace Cedar.Commands.Client
{
    using System;
    using Cedar.Commands.ExceptionModels;

    public interface IModelToExceptionConverter
    {
        Exception Convert(ExceptionModel model);
    }
}