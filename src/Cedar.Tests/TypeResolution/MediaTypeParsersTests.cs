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
                IParsedMediaAndSerializationType parsedMediaType;

                MediaTypeParsers.MediaTypeWithoutVersion("application/vnd.org.foo.bar+json", out parsedMediaType)
                    .Should().BeTrue();

                parsedMediaType.SerializationType.Should().Be("json");
                parsedMediaType.TypeName.Should().Be("org.foo.bar");
                parsedMediaType.Version.Should().Be(null);
            }

            [Fact]
            public void When_serilization_type_missing_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithoutVersion("application/vnd.org.foo.bar", out _)
                    .Should().BeFalse();
            }
        }

        public class MediaTypeWithDotVersionTests
        {
            [Fact]
            public void Can_parse_valid_MediaTypeWithoutVersion()
            {
                IParsedMediaAndSerializationType parsedMediaType;

                MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.org.foo.bar.v2+json", out parsedMediaType)
                    .Should().BeTrue();

                parsedMediaType.SerializationType.Should().Be("json");
                parsedMediaType.TypeName.Should().Be("org.foo.bar");
                parsedMediaType.Version.Should().Be(2);
            }

            [Fact]
            public void When_serilization_type_missing_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.org.foo.bar.v2", out _)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_version_missing_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.org.foo.bar+json", out _)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_version_malformed_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.org.foo.bar.vX+json", out _)
                    .Should().BeFalse();
            }
        }

        public class MediaTypeWithMinusVersionTests
        {
            [Fact]
            public void Can_parse_valid_MediaTypeWithoutVersion()
            {
                IParsedMediaAndSerializationType parsedMediaType;

                MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.org.foo.bar-v2+json", out parsedMediaType)
                    .Should().BeTrue();

                parsedMediaType.SerializationType.Should().Be("json");
                parsedMediaType.TypeName.Should().Be("org.foo.bar");
                parsedMediaType.Version.Should().Be(2);
            }

            [Fact]
            public void When_serilization_type_missing_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.org.foo.bar.-v2", out _)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_version_missing_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.org.foo.bar+json", out _)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_version_malformed_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.org.foo.bar-vX+json", out _)
                    .Should().BeFalse();
            }
        }

        public class MediaTypeWithQualiferVersionTests
        {
            [Fact]
            public void Can_parse_valid_MediaTypeWithoutVersion()
            {
                IParsedMediaAndSerializationType parsedMediaType;

                MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.org.foo.bar+json;v=2", out parsedMediaType)
                    .Should().BeTrue();

                parsedMediaType.SerializationType.Should().Be("json");
                parsedMediaType.TypeName.Should().Be("org.foo.bar");
                parsedMediaType.Version.Should().Be(2);
            }

            [Fact]
            public void When_serilization_type_missing_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.org.foo.bar;v=2", out _)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_version_missing_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.org.foo.bar+json", out _)
                    .Should().BeFalse();
            }

            [Fact]
            public void When_version_malformed_then_cant_parse()
            {
                IParsedMediaAndSerializationType _;
                MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.org.foo.bar+json;v=X", out _)
                    .Should().BeFalse();
            }
        }
    }
}