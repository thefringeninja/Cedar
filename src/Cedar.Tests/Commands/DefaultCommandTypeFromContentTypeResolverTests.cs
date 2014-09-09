namespace Cedar.Commands
{
    using System;
    using System.Linq;
    using Cedar.ContentNegotiation;
    using FluentAssertions;
    using Xunit;

    public class DefaultCommandTypeFromContentTypeResolverTests
    {
        [Fact]
        public void When_vendor_is_not_supplied_then_should_throw()
        {
            Action act = () => new DefaultContentTypeMapper(string.Empty, Enumerable.Empty<Type>());

            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void When_known_command_types_is_not_supplied_then_should_throw()
        {
            Action act = () => new DefaultContentTypeMapper("vendor", null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void When_command_is_unknown_then_should_throw()
        {
            var sut = new DefaultContentTypeMapper("vendor", Enumerable.Empty<Type>());

            Action act = () => sut.GetFromContentType("unknown");

            act.ShouldThrow<NotSupportedException>();
        }
    }
}