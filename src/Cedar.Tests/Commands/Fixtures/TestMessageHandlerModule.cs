namespace Cedar.Commands.Fixtures
{
    using System;
    using System.Threading.Tasks;

    public class TestHandlerModule : CommandHandlerModule
    {
        public TestHandlerModule()
        {
            For<TestCommand>()
                .Handle((_, __) => Task.FromResult(0));
            For<TestCommandWhoseHandlerThrows>()
                .Handle((_, __) => { throw new InvalidOperationException(); });
        }
    }
}