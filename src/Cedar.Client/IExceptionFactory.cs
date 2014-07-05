namespace Cedar.Client
{
    using System;

    public interface IExceptionFactory
    {
        Exception Create(ExceptionResponse exceptionResponse);
    }
}