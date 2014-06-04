namespace Cedar.CommandHandling.Serialization
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface ICommandDeserializer
    {
        bool Handles(string contentType);

        Task<object> Deserialize(Stream stream, Type commandType);
    }
}