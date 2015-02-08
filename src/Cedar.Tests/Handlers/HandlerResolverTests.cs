namespace Cedar.Handlers
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class CommandHandlerResolverTests
    {
        [Fact]
        public void Can_resolve_handler()
        {
            var module = new TestHandlerModule();
            var resolver = new HandlerResolver(module);

            var handlers = resolver.ResolveAll<TestEvent>();

            handlers.Should().HaveCount(1);
        }

        private class TestHandlerModule : HandlerModule
        {
            public TestHandlerModule()
            {
                For<TestEvent>()
                    .Handle((_, __) => Task.FromResult(0));
            }
        }

        private class TestEvent
        {}
    }
}