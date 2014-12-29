namespace Cedar.Commands
{
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class CommandHandlingSettingsTests
    {
        [Fact]
        public void When_set_serializera_to_null_then_should_throw()
        {
            var settings = new CommandHandlingSettings(A.Fake<ICommandHandlerResolver>(), A.Fake<ResolveCommandType>());

            Action act = () => settings.Serializer = null;

            act.ShouldThrow<ArgumentNullException>();
        }
    }
}