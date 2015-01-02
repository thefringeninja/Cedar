 // ReSharper disable once CheckNamespace
namespace Cedar.Commands.CommandTypeResolversTests
{
    using System;
    using Cedar.Commands.TypeResolution;
    using FluentAssertions;
    using Xunit.Extensions;

    public class FullNameWithUnderscoreVersionSuffixTests
    {
        [Theory]
        [InlineData(typeof(TestCommand), "cedar.commands.CommandTypeResolversTests.testcommand", null)]
        [InlineData(typeof(TestCommand), "cedar.commands.commandtyperesolverstests.testcommand", null)]
        [InlineData(typeof(TestCommand_v2), "cedar.commands.commandtyperesolverstests.testcommand", 2)]
        [InlineData(typeof(TestCommand_V2), "cedar.commands.commandtyperesolverstests.testcommand", 2)]
        public void Should_resolve_command(Type commandType, string commandName, int? version)
        {
            var sut = CommandTypeResolvers.FullNameWithUnderscoreVersionSuffix(new[] { commandType });

            var type = sut(commandName, version);

            type.Should().Be(commandType);
        }
    }

    public class FullNameWithVersionSuffixTests
    {
        [Theory]
        [InlineData(typeof(TestCommand), "cedar.commands.CommandTypeResolversTests.testcommand", null)]
        [InlineData(typeof(TestCommand), "cedar.commands.commandtyperesolverstests.testcommand", null)]
        [InlineData(typeof(TestCommandv2), "cedar.commands.commandtyperesolverstests.testcommand", 2)]
        [InlineData(typeof(TestCommandV2), "cedar.commands.commandtyperesolverstests.testcommand", 2)]
        public void Should_resolve_command(Type commandType, string commandName, int? version)
        {
            var sut = CommandTypeResolvers.FullNameWithVersionSuffix(new[] { commandType });

            var type = sut(commandName, version);

            type.Should().Be(commandType);
        }
    }

    public class AllTests
    {
        [Theory]
        [InlineData(typeof(TestCommand), "cedar.commands.CommandTypeResolversTests.testcommand", null)]
        [InlineData(typeof(TestCommand), "cedar.commands.commandtyperesolverstests.testcommand", null)]
        [InlineData(typeof(TestCommand_v2), "cedar.commands.commandtyperesolverstests.testcommand", 2)]
        [InlineData(typeof(TestCommand_V2), "cedar.commands.commandtyperesolverstests.testcommand", 2)]
        [InlineData(typeof(TestCommandv2), "cedar.commands.commandtyperesolverstests.testcommand", 2)]
        [InlineData(typeof(TestCommandV2), "cedar.commands.commandtyperesolverstests.testcommand", 2)]
        public void Should_resolve_command(Type commandType, string commandName, int? version)
        {
            var sut = CommandTypeResolvers.All(new[] { commandType });

            var type = sut(commandName, version);

            type.Should().Be(commandType);
        }
    }

    public class TestCommand { }

    public class TestCommand_v2 { }

    public class TestCommand_V2 { }

    public class TestCommandv2 { }

    public class TestCommandV2 { }
}
