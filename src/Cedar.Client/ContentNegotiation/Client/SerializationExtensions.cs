namespace Cedar.ContentNegotiation.Client
{
    using System;
    using System.IO;
    using System.Net.Http;

    public static class SerializationExtensions
    {
        public static string Serialize(this ISerializer serializer, object target)
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, target);

                return writer.ToString();
            }
        }

        public static object Deserialize(this ISerializer serializer, string target, Type type)
        {
            using (var reader = new StringReader(target))
            {
                return serializer.Deserialize(reader, type);
            }
        }
    }
}