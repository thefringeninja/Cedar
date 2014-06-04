namespace Cedar.CommandHandling.Serialization
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class JsonCommandDeserializerTests
    {
        [Fact]
        public void When_content_type_is_application_json_then_can_deserialize()
        {
            var deserializer = new JsonCommandDeserializer();
            deserializer.Handles(@"application/json").Should().BeTrue();
        }

        [Fact]
        public void When_content_type_is_vendor_with_json_then_can_deserialize()
        {
            var deserializer = new JsonCommandDeserializer();
            deserializer.Handles(@"application/vnd.cedar.thing+json").Should().BeTrue();
        }

        [Fact]
        public async Task Can_deserialize_stream_containing_json()
        {
            var deserializer = new JsonCommandDeserializer();
            using (var memoryStream = new MemoryStream())
            {
                byte[] bytes = Encoding.UTF8.GetBytes("{ \"name\" : \"damian\" }");
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Position = 0;

                object o = await deserializer.Deserialize(memoryStream, typeof(Person));

                o.Should().NotBeNull();
                o.Should().BeOfType<Person>();
            }
        }

        public class Person
        {
            string Name { get; set; }
        }
    }
}