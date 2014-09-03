namespace Cedar.Commands.Fixtures
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Handlers;

    public class TestHandlerModule : HandlerModule
    {
        public TestHandlerModule()
        {
            For<CommandMessage<TestCommand>>()
                .Handle((_, __) => Task.FromResult(0));
            For<CommandMessage<TestCommandWhoseHandlerThrows>>()
                .Handle((_, __) => { throw new InvalidOperationException(); });
        }
    }
}