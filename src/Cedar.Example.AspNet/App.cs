namespace Cedar.Example.AspNet
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.Handlers;
    using Cedar.NEventStore.Handlers;
    using Cedar.NEventStore.Handlers.TempImportFromNES;
    using Cedar.Queries;
    using Cedar.TypeResolution;
    using global::NEventStore;

    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    public class App : IDisposable
    {
        private static readonly Lazy<App> LazyApplicationInstance =
            new Lazy<App>(() => new App(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly InterlockedBoolean _isDisposed = new InterlockedBoolean();
        private readonly MidFunc _commandingMiddleware;
        private readonly IStoreEvents _storeEvents;
        private readonly DurableCommitDispatcher _durableCommitDispatcher;
        private readonly MidFunc _queryingMiddleware;

        private App()
        {
            var queryHandlerModule = new QueryHandlerModule();
            queryHandlerModule.For<Query, Query.Response>().HandleQuery((message, ct) => Task.FromResult(new Query.Response()));
            
            var settings = new HandlerSettings(
                queryHandlerModule,
                new DefaultRequestTypeResolver("cedar", new[] { typeof(Query), typeof(Query.Response) }));

            var commitDispatcherFailed = new TaskCompletionSource<Exception>();
            
            _commandingMiddleware = CommandHandlingMiddleware.HandleCommands(settings);
            _queryingMiddleware = QueryHandlingMiddleware.HandleQueries(settings);
            
            _storeEvents = Wireup.Init().UsingInMemoryPersistence().Build();
            var eventStoreClient = new EventStoreClient(_storeEvents.Advanced);

            _durableCommitDispatcher = new DurableCommitDispatcher(
                eventStoreClient,
                new InMemoryCheckpointRepository(),
                new HandlerModule(),
                TransientExceptionRetryPolicy.Indefinite(TimeSpan.FromMilliseconds(500)));

            _durableCommitDispatcher.ProjectedCommits.Subscribe(
                _ => { },
                commitDispatcherFailed.SetResult);

            _durableCommitDispatcher.Start().Wait();
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

        public MidFunc CommandingMiddleWare
        {
            get { return _commandingMiddleware; }
        }
        public MidFunc QueryingMiddleWare
        {
            get { return _queryingMiddleware; }
        } 
    }
}