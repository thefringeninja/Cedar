namespace Cedar.Commands.Fixtures
{
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;

    public class TestCommandHandler : IHandler<CommandMessage<TestCommand>>
    {
        public Task Handle(CommandMessage<TestCommand> commandMessage, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}