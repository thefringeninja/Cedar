namespace Cedar.TypeResolution
{
    using Cedar.Owin;
    using FluentAssertions;
    using Xunit;

    public class TypeResolverVersioningTests
    {
        private readonly DefaultRequestTypeResolver _sut;

        public TypeResolverVersioningTests()
        {
            _sut = new DefaultRequestTypeResolver("cedar.tests",
                new[]
                {
                    typeof (SomeCommand), typeof (SomeCommand_v12), typeof (SomeCommand_v2), typeof (SomeCommand_v25), typeof (SomeCommand_v26)
                });
            
        }
        [Fact]
        public void Should_get_latest_when_version_is_omitted()
        {
            _sut.ResolveInputType(CreateRequestFromContentType("application/vnd.cedar.tests.somecommand+json"))
                .Should().Be(typeof (SomeCommand_v26));
        }

        [Fact]
        public void Should_get_next_most_recent_version_when_no_match_found()
        {
            _sut.ResolveInputType(CreateRequestFromContentType("application/vnd.cedar.tests.somecommand.v3+json"))
                .Should().Be(typeof(SomeCommand_v12));
        }

        [Fact]
        public void Should_get_version_1_even_if_class_is_not_so_named()
        {
            _sut.ResolveInputType(CreateRequestFromContentType("application/vnd.cedar.tests.somecommand.v1+json"))
                .Should().Be(typeof(SomeCommand));
        }

        private static IRequest CreateRequestFromContentType(string contentType)
        {
            var context = new OwinContext
            {
                Request =
                {
                    Protocol = "HTTP/1.1",
                    Scheme = "http",
                    Host = new HostString("locahost"),
                    ContentType = contentType
                }
            };
            
            return new CedarRequest(context);
        }

        class SomeCommand { }
        class SomeCommand_v2 { }
        class SomeCommand_v12 { }
        class SomeCommand_v25 { }
        class SomeCommand_v26 { }
        
    }
}