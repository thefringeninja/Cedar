namespace Cedar.Hosting
{
    using System;
    using System.Threading.Tasks;
    using Cedar.CommandHandling;
    using Cedar.CommandHandling.Dispatching;
    using FluentAssertions;
    using TinyIoC;
    using Xunit;

    public class TinyIoCCommandHandlerResolverTests
    {
        private readonly TinyIoCContainer _container;
        private readonly TinyIoCCommandHandlerResolver _sut;

        public TinyIoCCommandHandlerResolverTests()
        {
            _container = new TinyIoCContainer();
            _sut = new TinyIoCCommandHandlerResolver(_container);
        }

        [Fact]
        public void More_that_one_handler_registered_throws_an_exception()
        {
            _container.RegisterCommandHandler<Test, TestCommandHandler>();
            _container.RegisterCommandHandler<Test, AnotherTestCommandHandler>();

            Action act = () => _sut.Resolve<Test>();

            act.ShouldThrow<InvalidOperationException>();
        }

        private class AnotherTestCommandHandler : ICommandHandler<Test>
        {
            public Task Handle(ICommandContext context, Test command)
            {
                return Task.FromResult(true);
            }
        }

        private class Test
        {}

        private class TestCommandHandler : ICommandHandler<Test>
        {
            public Task Handle(ICommandContext context, Test command)
            {
                return Task.FromResult(true);
            }
        }
    }
}