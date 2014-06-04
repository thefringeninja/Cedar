namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cedar.Client;
    using Cedar.CommandHandling.Dispatching;
    using Owin;
    using Xunit;

    public class CommandHandlingTests
    {
        [Fact]
        public async Task Blah()
        {
            using (var host = new CedarHost(new TestBootstrapper()))
            {
                using (var client = new CedarClient(new Uri("http://localhost"), new OwinHttpMessageHandler(host.AppFunc)))
                {
                    await client.ExecuteCommand(new TestCommand(), Guid.NewGuid());
                }
            }
        }
    }

    public class TestBootstrapper : CedarBootstrapper
    {
        public override IEnumerable<Type> CommandHandlerTypes
        {
            get { return new[] {typeof (TestCommandHandler)}; }
        }

        public override string VendorName
        {
            get { return "CommandHandlingTests"; }
        }
    }

    public class TestCommand
    {}

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task Handle(ICommandContext context, TestCommand command)
        {
            throw new NotImplementedException();
        }
    }
}