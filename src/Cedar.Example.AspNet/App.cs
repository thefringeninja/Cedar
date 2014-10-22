namespace Cedar.Example.AspNet
{
    using System;
    using System.Linq;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.Handlers;
    using Cedar.Handlers.TempImportFromNES;
    using Cedar.Internal;
    using Cedar.Queries;
    using Cedar.TypeResolution;
    using Microsoft.Owin;
    using NEventStore;
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
            
            var settings = new DefaultHandlerSettings(
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

        private MidFunc CreateGate(Task<Exception> commitDispatcherFailed)
        {
            return 
                next =>
                async env =>
                {
                    if (commitDispatcherFailed.IsCompleted) 
                    { 
                        var owinContext = new OwinContext(env);
                        owinContext.Response.StatusCode = 500;
                        owinContext.Response.ReasonPhrase = "Internal Server Error";
                        owinContext.Response.Write(commitDispatcherFailed.Result.ToString());
                        return;
                    }
                    await next(env);
                };
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