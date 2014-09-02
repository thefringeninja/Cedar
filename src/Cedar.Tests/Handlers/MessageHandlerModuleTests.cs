namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class MessageHandlerModuleTests
    {
        [Fact]
        public void Can_handle_message()
        {
            var handlerModule = new TestMessageHandlerModule();

            IEnumerable<Handler<TestMessage>> handlersFor = handlerModule.GetHandlersFor<TestMessage>();
            foreach (var handler in handlersFor)
            {
                handler(new TestMessage(), CancellationToken.None);
            }

            handlerModule.MiddlewareCalled.Should().BeTrue();
            handlerModule.FinallyCalled.Should().BeTrue();
        }

        private class TestMessage
        {}

        private class TestMessageHandlerModule : MessageHandlerModule
        {
            private bool _finallyCalled;
            private bool _middlewareCalled;

            public TestMessageHandlerModule()
            {
                ForMessage<TestMessage>()
                    .Handle(next => (message, ct) =>
                    {
                        _middlewareCalled = true;
                        return next(message, ct);
                    })
                    .Finally((message, ct) =>
                    {
                        _finallyCalled = true;
                        return Task.FromResult(0);
                    });
            }

            public bool MiddlewareCalled
            {
                get { return _middlewareCalled; }
            }

            public bool FinallyCalled
            {
                get { return _finallyCalled; }
            }
        }
    }
}