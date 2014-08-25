namespace Cedar.CommandHandling.Fixtures
{
    using System;
    using System.Threading.Tasks;

    public class TestCommandWhoseHandlerThrowsHandler : ICommandHandler<TestCommandWhoseHandlerThrows>
    {
        public Task Handle(ICommandContext context, TestCommandWhoseHandlerThrows command)
        {
            throw new NotSupportedException();
        }
    }
}