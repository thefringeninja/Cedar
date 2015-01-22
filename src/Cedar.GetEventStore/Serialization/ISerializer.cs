namespace Cedar.GetEventStore.Serialization
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents an ability to serialize and deserialze an object.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Deserializes on object from the specified reader.
        /// </summary>
        /// <param name="reader">The reader which contains the object to be deserialized.</param>
        /// <param name="type">The type of object to be deserialized.</param>
        /// <returns></returns>
        object Deserialize(TextReader reader, Type type);

        /// <summary>
        /// Serializes the specified writer.
        /// </summary>
        /// <param name="writer">The writer an object is serialized to.</param>
        /// <param name="source">The source object to serialize.</param>
        void Serialize(TextWriter writer, object source);
    }
}