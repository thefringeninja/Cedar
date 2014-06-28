namespace Cedar.CommandHandling.Modules
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class CommandTypeResolverTests
    {
        [Fact]
        public void Should_get_command_type()
        {
            var resolver = new CommandTypeFromContentTypeResolver("cedar.tests", new[] {typeof (CommandTypeResolverTests)});

            Type commandType = resolver.GetCommandType(@"application/vnd.cedar.tests.commandtyperesolvertests+json");

            commandType.Should().NotBeNull();
        }
    }
}