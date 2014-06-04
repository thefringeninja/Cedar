namespace Cedar.CommandHandling.ExceptionHandling
{
    using System;
    using FluentAssertions;
    using Nancy;
    using Nancy.Responses.Negotiation;
    using Xunit;

    public class CommandExceptionHandlerTests
    {
        private readonly TestCommandExceptionHandler _handler;

        public CommandExceptionHandlerTests()
        {
            _handler = new TestCommandExceptionHandler();
        }

        [Fact]
        public void Should_handle_defined_exception()
        {
            _handler.Handles(new ApplicationException()).Should().BeTrue();
        }

        [Fact]
        public void Should_not_handle_base_exception()
        {
            _handler.Handles(new Exception()).Should().BeFalse();
        }

        [Fact]
        public void Should_not_handle_derived_exception()
        {
            _handler.Handles(new DerivedException()).Should().BeFalse();
        }

        [Fact]
        public void When_handling_exception_should_call_handle()
        {
            var exception = new ApplicationException();
            var negotiator = new Negotiator(new NancyContext());

            Negotiator negotiatorReturned = ((ICommandExceptionHandler) _handler).Handle(exception, negotiator);

            _handler.Exception.Should().Be(exception);
            negotiatorReturned.Should().Be(negotiator);
        }

        private class TestCommandExceptionHandler : CommandExceptionHandler<ApplicationException>
        {
            private ApplicationException _exception;

            public ApplicationException Exception
            {
                get { return _exception; }
            }

            protected override Negotiator Handle(ApplicationException exception, Negotiator negotiator)
            {
                _exception = exception;
                return negotiator;
            }
        }

        private class DerivedException : ApplicationException { }
    }
}