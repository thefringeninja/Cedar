namespace Cedar.CommandHandling.Serialization
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class XmlCommandDeserializerTests
    {
        [Fact]
        public void When_content_type_is_application_xml_then_can_deserialize()
        {
            var deserializer = new XmlCommandDeserializer();
            deserializer.Handles(@"application/xml").Should().BeTrue();
        }

        [Fact]
        public void When_content_type_is_vendor_with_xml_then_can_deserialize()
        {
            var deserializer = new XmlCommandDeserializer();
            deserializer.Handles(@"application/vnd.cedar.thing+xml").Should().BeTrue();
        }

        [Fact]
        public async Task Can_deserialize_stream_containing_xml()
        {
            var deserializer = new XmlCommandDeserializer();
            using (var memoryStream = new MemoryStream())
            {
                byte[] bytes = Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><Person><Name>Damian</Name></Person>");
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Position = 0;

                object o = await deserializer.Deserialize(memoryStream, typeof(Person));

                o.Should().NotBeNull();
                o.Should().BeOfType<Person>();
            }
        }

        [Serializable]
        public class Person
        {
            string Name { get; set; }
        }
    }
}