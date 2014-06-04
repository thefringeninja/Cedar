namespace Cedar.CommandHandling.Serialization
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public class XmlCommandDeserializer : ICommandDeserializer
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> Serializers
            = new ConcurrentDictionary<Type, XmlSerializer>();

        public bool Handles(string contentType)
        {
            return contentType.Equals(@"application/xml", StringComparison.OrdinalIgnoreCase)
                   || contentType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<object> Deserialize(Stream stream, Type commandType)
        {
            XmlSerializer serializer = Serializers.GetOrAdd(commandType, _ => new XmlSerializer(commandType));
            using (var memorySream = new MemoryStream())
            {
                await stream.CopyToAsync(memorySream);
                memorySream.Position = 0;
                return serializer.Deserialize(memorySream);
            }
        }
    }
}