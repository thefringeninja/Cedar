namespace Cedar.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class HandlerResolverDispatcherTests
    {
        [Fact]
        public async Task Should_dispatch_message_to_handler()
        {
            var handlerResolver = A.Fake<IHandlerResolver>();
            var messageHandler = A.Fake<IHandler<string>>();
            A.CallTo(() => messageHandler.Handle("Test", CancellationToken.None))
                .Returns(Task.FromResult(0));
            A.CallTo(() => handlerResolver.ResolveAll<string>())
                .Returns(new[] {messageHandler});

            await handlerResolver.Dispatch("Test", CancellationToken.None);

            A.CallTo(() => messageHandler.Handle("Test", CancellationToken.None))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}