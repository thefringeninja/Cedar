namespace Cedar.CommandHandling.Fixtures
{
    using System.Threading.Tasks;

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task Handle(ICommandContext context, TestCommand command)
        {
            return Task.FromResult(0);
        }
    }
}