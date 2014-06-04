namespace Cedar.CommandHandling.Modules
{
    using System;

    public interface ICommandTypeResolver
    {
        Type GetCommandType(string contentType);
    }
}