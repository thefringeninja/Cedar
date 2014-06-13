using System;
using System.Threading.Tasks;
using Cedar.CommandHandling;
using Cedar.CommandHandling.Dispatching;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using FluentAssertions;
using Nancy.TinyIoc;

namespace Cedar.Hosting
{
    using Xunit;
    public class TinyIoCCommandHandlerResolverTests
    {
        private TinyIoCCommandHandlerResolver _sut;
        private TinyIoCContainer _container;

        public TinyIoCCommandHandlerResolverTests()
        {
            _container = new TinyIoCContainer();
            _sut = new TinyIoCCommandHandlerResolver(_container);
        }

        [Fact]
        public void More_that_one_handler_registered_throws_an_exception()
        {
            CedarHost.RegisterCommandHander<Test, TestCommandHandler>(_container);
            CedarHost.RegisterCommandHander<Test, AnotherTestCommandHandler>(_container);

            Action act = () => _sut.Resolve<Test>();

            act.ShouldThrow<InvalidOperationException>();
        }

        class Test
        {
            
        }

        class TestCommandHandler : ICommandHandler<Test>
        {
            public Task Handle(ICommandContext context, Test command)
            {
                return Task.FromResult(true);
            }
        }
        class AnotherTestCommandHandler : ICommandHandler<Test>
        {
            public Task Handle(ICommandContext context, Test command)
            {
                return Task.FromResult(true);
            }
        }
    }
}
