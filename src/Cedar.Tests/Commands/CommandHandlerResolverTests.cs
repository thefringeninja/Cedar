namespace Cedar.Commands
{
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using FluentAssertions;
    using Xunit;

    public class CommandHandlerResolverTests
    {
        [Fact]
        public void Can_resolve_handler()
        {
            var module = new TestCommandHandlerModule();
            var resolver = new CommandHandlerResolver(module);

            Handler<CommandMessage<TestCommand>> handler = resolver.Resolve<TestCommand>();

            handler.Should().NotBeNull();
        }

        private class TestCommandHandlerModule : CommandHandlerModule
        {
            public TestCommandHandlerModule()
            {
                For<TestCommand>()
                    .Handle((_, __) => Task.FromResult(0));
            }
        }

        public class TestCommand { }
    }
}
