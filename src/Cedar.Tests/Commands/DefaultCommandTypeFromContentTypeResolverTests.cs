namespace Cedar.Commands
{
    using System;
    using System.IO;
    using System.Linq;
    using Cedar.TypeResolution;
    using FluentAssertions;
    using Xunit;

    public class DefaultCommandTypeFromContentTypeResolverTests
    {
        [Fact]
        public void When_vendor_is_not_supplied_then_should_throw()
        {
            Action act = () => new DefaultRequestTypeResolver(string.Empty, Enumerable.Empty<Type>());

            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void When_known_command_types_is_not_supplied_then_should_throw()
        {
            Action act = () => new DefaultRequestTypeResolver("vendor", null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void When_command_is_unknown_then_should_return_null()
        {
            var sut = new DefaultRequestTypeResolver("vendor", Enumerable.Empty<Type>());

            sut.ResolveInputType(new FakeRequest())
                .Should().BeNull();
        }

        class FakeRequest : IRequest
        {
            private readonly Uri _uri = new Uri("http://localhost/");
            private readonly ILookup<string, string> _headers = new string[]{}.ToLookup(x => x);

            public Uri Uri
            {
                get { return _uri; }
            }

            public ILookup<string, string> Headers
            {
                get { return _headers; }
            }

            public Stream Body
            {
                get { return new MemoryStream(); }
            }
        }
    }
}