namespace Cedar.Example.AspNet
{
    using System;
    using System.Linq;
    using System.Threading;
    using Cedar.Commands;
    using Cedar.Handlers;
    using Cedar.Internal;
    using Cedar.TypeResolution;
    using NEventStore;
    using NEventStore.Client;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    public class App : IDisposable
    {
        private static readonly Lazy<App> LazyApplicationInstance =
            new Lazy<App>(() => new App(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly InterlockedBoolean _isDisposed = new InterlockedBoolean();
        private readonly MidFunc _middleware;
        private readonly IStoreEvents _storeEvents;
        private readonly DurableCommitDispatcher _durableCommitDispatcher;

        private App()
        {
            var settings = new DefaultHandlerSettings(
                new HandlerModule(),
                new DefaultRequestTypeResolver("cedar", Enumerable.Empty<Type>()));
            _middleware = CommandHandlingMiddleware.HandleCommands(settings);

            _storeEvents = Wireup.Init().UsingInMemoryPersistence().Build();
            var eventStoreClient = new EventStoreClient(new PollingClient(_storeEvents.Advanced));

            _durableCommitDispatcher = new DurableCommitDispatcher(
                eventStoreClient,
                new InMemoryCheckpointRepository(),
                new HandlerModule(),
                TransientExceptionRetryPolicy.Indefinite(TimeSpan.FromMilliseconds(500)));

            _durableCommitDispatcher.ProjectedCommitsStream.Subscribe(
                _ => { },
                ex =>
                {
                    //TODO a handler has failed fatally, need to shut down application.
                });

            Initialize();
        }

        private async void Initialize()
        {
            await _durableCommitDispatcher.Start();
        }

        public static App Instance
        {
            get { return LazyApplicationInstance.Value; }
        }

        public void Dispose()
        {
            if (_isDisposed.EnsureCalledOnce())
            {
                return;
            }
            _durableCommitDispatcher.Dispose();
            _storeEvents.Dispose();
        }

        public MidFunc Middleware
        {
            get { return _middleware; }
        } 
    }
}