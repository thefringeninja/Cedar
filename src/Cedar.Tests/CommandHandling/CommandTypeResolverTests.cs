namespace Cedar.CommandHandling
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class CommandTypeResolverTests
    {
        [Fact]
        public void Should_get_command_type()
        {
            var resolver = new DefaultCommandTypeFromContentTypeResolver("cedar.tests", new[] {typeof (CommandTypeResolverTests)});

            Type commandType = resolver.GetFromContentType(@"application/vnd.cedar.tests.commandtyperesolvertests+json");

            commandType.Should().NotBeNull();
        }
    }
}