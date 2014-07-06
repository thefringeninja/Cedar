namespace Cedar.CommandHandling
{
    using System;
    using Cedar.Exceptions;

    public interface IExceptionToModelConverter
    {
        ExceptionModel Convert(Exception exception);
    }
}