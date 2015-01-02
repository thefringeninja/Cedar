namespace Cedar.Commands
{
    using System;
    using Cedar.Commands.TypeResolution;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class CommandHandlingSettingsTests
    {
        [Fact]
        public void When_set_serializer_to_null_then_should_throw()
        {
            var sut = new CommandHandlingSettings(A.Fake<ICommandHandlerResolver>(), A.Fake<ResolveCommandType>());

            Action act = () => sut.ResolveSerializer = null;

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Should_have_default_serializer_resolver()
        {
            var sut = new CommandHandlingSettings(A.Fake<ICommandHandlerResolver>(), A.Fake<ResolveCommandType>());

            sut.ResolveSerializer.Should().NotBeNull();
        }

        [Fact]
        public void Can_set_ParseMediaType()
        {
            var sut = new CommandHandlingSettings(A.Fake<ICommandHandlerResolver>(), A.Fake<ResolveCommandType>());
            var parseMediaType = A.Fake<ParseMediaType>();

            sut.ParseMediaType = parseMediaType;

            sut.ParseMediaType.Should().Be(parseMediaType);
        }

        [Fact]
        public void Can_set_ResolveSerializer()
        {
            var sut = new CommandHandlingSettings(A.Fake<ICommandHandlerResolver>(), A.Fake<ResolveCommandType>());
            var resolveSerializer = A.Fake<ResolveSerializer>();

            sut.ResolveSerializer = resolveSerializer;

            sut.ResolveSerializer.Should().Be(resolveSerializer);
        }
    }
}