namespace Cedar.Commands
{
    using Cedar.Commands.Fixtures;
    using FluentAssertions;
    using Xunit;

    public class CommandTypeResolversTests
    {
        [Fact]
        public void Can_resolve_type_without_version()
        {
            var sut = CommandTypeResolvers.FullNameWithVersionSuffix(new[] { typeof(TestCommand), typeof(TestCommand_v2) });

            var type = sut("cedar.commands.fixtures.testcommand", null);

            type.Should().Be<TestCommand>();
        }

        [Fact]
        public void Can_resolve_type_with_version()
        {
            var sut = CommandTypeResolvers.FullNameWithVersionSuffix(new[] { typeof(TestCommand), typeof(TestCommand_v2) });

            var type = sut("cedar.commands.fixtures.testcommand", 2);

            type.Should().Be<TestCommand_v2>();
        }
    }
}
