namespace Cedar.CommandHandling
{
    using System;

    public interface IExceptionToModelConverter
    {
        object Convert(Exception exception);
    }
}