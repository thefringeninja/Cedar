namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cedar.Client;
    using Cedar.CommandHandling.Dispatching;
    using FluentAssertions;
    using Xunit;

    public class CustomExceptionTests
    {
        [Fact]
        public void When_execute_valid_command_then_should_not_throw()
        {
            using (var host = new CedarHost(new TestBootstrapper()))
            {
                var exceptionFactory = new DelegateExceptionFactory(r =>
                    r.ExeptionType == typeof (CustomException).Name ? new CustomException(r.Message) : null);
                using (CedarClient client = host.CreateClient(exceptionFactory))
                {
                    Func<Task> act = () => client.ExecuteCommand("cedar", new TestCommand(), Guid.NewGuid());

                    act.ShouldThrow<CustomException>();
                }
            }
        }

        public class TestBootstrapper : CedarBootstrapper
        {
            public override string VendorName
            {
                get { return "cedar"; }
            }

            public override IEnumerable<Type> CommandHandlerTypes
            {
                get { return new[] {typeof (TestCommandHandler)}; }
            }
        }

        public class TestCommand
        {}

        public class TestCommandHandler : ICommandHandler<TestCommand>
        {
            public Task Handle(ICommandContext context, TestCommand command)
            {
                throw new CustomException("custom");
            }
        }

        public class CustomException : Exception
        {
            public CustomException(string message)
                : base(message)
            {}
        }
    }
}