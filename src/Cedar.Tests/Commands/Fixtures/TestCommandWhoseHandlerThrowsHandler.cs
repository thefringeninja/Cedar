namespace Cedar.Commands.Fixtures
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;

    public class TestCommandWhoseHandlerThrowsHandler : IHandler<CommandMessage<TestCommandWhoseHandlerThrows>>
    {
        public Task Handle(CommandMessage<TestCommandWhoseHandlerThrows> commandMessage, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}