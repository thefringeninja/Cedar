namespace Cedar.MessageHandling
{
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using FakeItEasy;
    using Xunit;

    public class MessageDispatcherTests
    {
        [Fact]
        public async Task Should_dispatch_message_to_handler()
        {
            var messageHandlerResolver = A.Fake<IHandlerResolver>();
            var messageHandler = A.Fake<IHandler<string>>();
            A.CallTo(() => messageHandler.Handle("Test", CancellationToken.None))
                .Returns(Task.FromResult(0));
            A.CallTo(() => messageHandlerResolver.ResolveAll<string>())
                .Returns(new[] {messageHandler});
            var messageDispatcher = new Dispatcher(messageHandlerResolver);

            await messageDispatcher.Message("Test", CancellationToken.None);

            A.CallTo(() => messageHandler.Handle("Test", CancellationToken.None))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}