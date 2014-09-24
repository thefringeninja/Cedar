namespace Cedar.Serialization.Client
{
    using System;
    using System.IO;

    public interface ISerializer
    {
        object Deserialize(TextReader reader, Type type);

        void Serialize(TextWriter writer, object target);
    }
}