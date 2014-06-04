namespace CED.Domain
{
    using System;

    public interface ICommandTypeResolver
    {
        Type GetCommandType(string commandType, string contentType);
    }
}