namespace Cedar.Client
{
    using System;
    using Cedar.Exceptions;

    public interface IModelToExceptionConverter
    {
        Exception Convert(ExceptionModel model);
    }
}