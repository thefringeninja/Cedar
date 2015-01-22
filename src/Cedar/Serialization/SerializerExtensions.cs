namespace Cedar.Serialization
{
    using System;
    using System.IO;

    /// <summary>
    /// Provides extension methods for the <see cref="ISerializer"/> interface.
    /// </summary>
    public static class SerializerExtensions
    {
        /// <summary>
        /// Serializes the specified source object to a string.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="source">The source object to be serialized.</param>
        /// <returns>A string representing the source object.</returns>
        public static string Serialize(this ISerializer serializer, object source)
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, source);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Deserializes an object from the.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="source">The source string representing the serialized object.</param>
        /// <param name="type">The type to deserialize to.</param>
        /// <returns></returns>
        public static object Deserialize(this ISerializer serializer, string source, Type type)
        {
            using (var reader = new StringReader(source))
            {
                return serializer.Deserialize(reader, type);
            }
        }

        /// <summary>
        /// Deserializes an object from the specified source.
        /// </summary>
        /// <typeparam name="T">The type of object to be deserialized.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <param name="source">The source string representing the serialized object.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(this ISerializer serializer, string source)
        {
            using (var reader = new StringReader(source))
            {
                return (T)serializer.Deserialize(reader, typeof(T));
            }
        }
    }
}