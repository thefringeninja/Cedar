namespace Cedar.TypeResolution
{
    using Cedar.Annotations;
    using FluentAssertions;
    using Xunit;

    [UsedImplicitly]
    public class MediaTypeParsersTests
    {
        public class MediaTypeWithoutVersionTests
        {
            [Fact]
            public void Can_parse_valid_MediaTypeWithoutVersion()
            {
                var parsedMediaType = MediaTypeParsers.MediaTypeWithoutVersion("application/vnd.org.foo.bar+json");

                parsedMediaType.SerializationType
                    .Should().Be("json");
                parsedMediaType.TypeName
                    .Should().Be("org.foo.bar");
                parsedMediaType.Version
                    .Should().Be(null);
            }

            [Fact]
            public void When_serilization_type_missing_then_cant_parse()
            {
                MediaTypeParsers.MediaTypeWithoutVersion("application/vnd.org.foo.bar")
                    .Should().BeNull();
            }
        }

        public class MediaTypeWithDotVersionTests
        {
            [Fact]
            public void Can_parse_valid_MediaTypeWithoutVersion()
            {
                var parsedMediaType = MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.org.foo.bar.v2+json");

                parsedMediaType.SerializationType
                    .Should().Be("json");
                parsedMediaType.TypeName
                    .Should().Be("org.foo.bar");
                parsedMediaType.Version
                    .Should().Be(2);
            }

            [Fact]
            public void When_serilization_type_missing_then_cant_parse()
            {
                MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.org.foo.bar.v2")
                    .Should().BeNull();
            }

            [Fact]
            public void When_version_missing_then_cant_parse()
            {
                MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.org.foo.bar+json")
                    .Should().BeNull();
            }

            [Fact]
            public void When_version_malformed_then_cant_parse()
            {
                MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.org.foo.bar.vX+json")
                    .Should().BeNull();
            }
        }

        public class MediaTypeWithMinusVersionTests
        {
            [Fact]
            public void Can_parse_valid_MediaTypeWithoutVersion()
            {
                var parsedMediaType = MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.org.foo.bar-v2+json");

                parsedMediaType.SerializationType
                    .Should().Be("json");
                parsedMediaType.TypeName
                    .Should().Be("org.foo.bar");
                parsedMediaType.Version
                    .Should().Be(2);
            }

            [Fact]
            public void When_serilization_type_missing_then_cant_parse()
            {
                MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.org.foo.bar.-v2")
                    .Should().BeNull();
            }

            [Fact]
            public void When_version_missing_then_cant_parse()
            {
                MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.org.foo.bar+json")
                    .Should().BeNull();
            }

            [Fact]
            public void When_version_malformed_then_cant_parse()
            {
                MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.org.foo.bar-vX+json")
                    .Should().BeNull();
            }
        }

        public class MediaTypeWithQualiferVersionTests
        {
            [Fact]
            public void Can_parse_valid_MediaTypeWithoutVersion()
            {
                var parsedMediaType = MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.org.foo.bar+json;v=2");

                parsedMediaType.SerializationType
                    .Should().Be("json");
                parsedMediaType.TypeName
                    .Should().Be("org.foo.bar");
                parsedMediaType.Version
                    .Should().Be(2);
            }

            [Fact]
            public void When_serilization_type_missing_then_cant_parse()
            {
                MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.org.foo.bar;v=2")
                    .Should().BeNull();
            }

            [Fact]
            public void When_version_missing_then_cant_parse()
            {
                IParsedMediaType _;
                MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.org.foo.bar+json")
                    .Should().BeNull();
            }

            [Fact]
            public void When_version_malformed_then_cant_parse()
            {
                MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.org.foo.bar+json;v=X")
                    .Should().BeNull();
            }
        }
    }
}