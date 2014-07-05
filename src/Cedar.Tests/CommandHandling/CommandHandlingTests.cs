namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cedar.Client;
    using Cedar.CommandHandling.Dispatching;
    using FluentAssertions;
    using Xunit;

    public class CommandHandlingTests
    {
        [Fact]
        public void When_execute_valid_command_then_should_not_throw()
        {
            using (var host = new CedarHost(new TestBootstrapper()))
            {
                using (var client = host.CreateClient())
                {
                    Func<Task> act = () => client.ExecuteCommand("cedar", new TestCommand(), Guid.NewGuid());
                    
                    act.ShouldNotThrow();
                }
            }
        }

        [Fact]
        public void When_execute_command_without_handler_then_should_throw()
        {
            using (var host = new CedarHost(new TestBootstrapper()))
            {
                using (var client = host.CreateClient())
                {
                    Func<Task> act = () => client.ExecuteCommand("cedar", new TestCommandWithoutHandler(), Guid.NewGuid());
                    
                    act.ShouldThrow<InvalidOperationException>();
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
                get { return new [] { typeof(TestCommandHandler) }; }
            }
        }
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task Handle(ICommandContext context, TestCommand command)
        {
            return Task.FromResult(0);
        }
    }

    public class TestCommand { }

    public class TestCommandWithoutHandler { }
}