namespace Cedar.CommandHandling.Dispatching
{
    using System;
    using System.Threading.Tasks;
    using Cedar.CommandHandling;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class CommandDispatcherTests
    {
        private readonly CommandDispatcher _sut;
        private readonly ICommandHandlerResolver _fakeHandlerResolver;

        public CommandDispatcherTests()
        {
            _fakeHandlerResolver = A.Fake<ICommandHandlerResolver>();
            _sut = new CommandDispatcher(_fakeHandlerResolver);
        }

        [Fact]
        public async Task When_dispatching_then_should_resolve_handler()
        {
            var command = new object();
            await _sut.Dispatch(A.Fake<ICommandContext>(), command);

            A.CallTo(() => _fakeHandlerResolver.Resolve<object>()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task When_dispatching_then_should_invoke_handler()
        {
            var fakeHandler = A.Fake<ICommandHandler<object>>();
            A.CallTo(() => _fakeHandlerResolver.Resolve<object>()).Returns(fakeHandler);
            var fakeCommandContext = A.Fake<ICommandContext>();
            var command = new object();

            await _sut.Dispatch(fakeCommandContext, command);

            A.CallTo(() => fakeHandler.Handle(fakeCommandContext, command)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void When_handler_not_found_then_should_throw()
        {
            A.CallTo(() => _fakeHandlerResolver.Resolve<object>()).Returns(null);
            var fakeCommandContext = A.Fake<ICommandContext>();
            var command = new object();

            Func<Task> act = () => _sut.Dispatch(fakeCommandContext, command);
            act.ShouldThrow<InvalidOperationException>();
        }
    }
}