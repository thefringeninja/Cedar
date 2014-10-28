namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Handlers;
    using Cedar.ProcessManagers.Persistence;

    public static class ProcessHandler
    {
        public static ProcessHandler<TProcess> For<TProcess>(
            IHandlerResolver commandDispatcher,
            ClaimsPrincipal principal,
            IProcessManagerFactory processManagerFactory,
            Func<string, string> buildProcessId = null) where TProcess : IProcessManager
        {
            return new ProcessHandler<TProcess>(commandDispatcher, principal, processManagerFactory, buildProcessId);
        }
    }

    public class ProcessHandler<TProcess> where TProcess : IProcessManager
    {
        // ReSharper disable StaticFieldInGenericType
        private static readonly MethodInfo DispatchCommandMethodInfo = typeof(HandlerModulesDispatchCommand)
            .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.Public);
        // ReSharper restore StaticFieldInGenericType

        private readonly IList<Pipe<object>> _pipes;
        private readonly ProcessManagerDispatcher _dispatcher;

        internal ProcessHandler(IHandlerResolver commandDispatcher, ClaimsPrincipal principal, IProcessManagerFactory processManagerFactory, Func<string, string> buildProcessId = null)
        {
            _pipes = new List<Pipe<object>>();
            _dispatcher = new ProcessManagerDispatcher(commandDispatcher, principal, processManagerFactory, buildProcessId);
        }

        public ProcessHandler<TProcess> CorrelateBy<TMessage>(
            Func<TMessage, string> getCorrelationId)
        {
            _dispatcher.CorrelateBy(getCorrelationId);

            return this;
        }

        public ProcessHandler<TProcess> Pipe(Pipe<object> pipe)
        {
            _pipes.Add(pipe);

            return this;
        }

        public IHandlerResolver BuildHandlerResolver(Handler<object> handler)
        {
            return _dispatcher
                .Aggregate(new HandlerModule(), (module, type) => HandleMessageType(module, type, handler));
        }

        private HandlerModule HandleMessageType(HandlerModule module, Type messageType, Handler<object> handler)
        {
            return (HandlerModule)GetType()
                .GetMethod("BuildHandler", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(messageType)
                .Invoke(this, new object[] { module, handler });
        }

        [UsedImplicitly]
        private HandlerModule BuildHandler<TMessage>(HandlerModule module, Handler<object> handler)
        {
            _pipes.Select(pipe => Delegate.CreateDelegate(typeof(Pipe<TMessage>), pipe.Method) as Pipe<TMessage>)
                .Aggregate(module.For<TMessage>(), (builder, pipe) => builder.Pipe(pipe))
                .Handle((message, ct) => handler(message, ct));

            return module;
        }

        public delegate Task<TProcess> GetProcess(string bucketId, string processId, CancellationToken token);

        public delegate Task SaveProcess(string bucketId, TProcess process, CancellationToken token);

        class ProcessManagerDispatcher : IHandlerResolver, IEnumerable<Type>, IObservable<CheckpointReached>
        {
            private readonly IHandlerResolver _commandDispatcher;
            private readonly ClaimsPrincipal _principal;
            private readonly IProcessManagerFactory _processManagerFactory;
            private readonly Func<string, string> _buildProcessId;
            private readonly IDictionary<Type, Func<object, string>> _byCorrelationId;
            private readonly ConcurrentDictionary<string, TProcess> _activeProcesses;
            private readonly ISubject<CheckpointReached> _outbox; 

            public ProcessManagerDispatcher(IHandlerResolver commandDispatcher, ClaimsPrincipal principal, IProcessManagerFactory processManagerFactory, Func<string, string> buildProcessId = null)
            {
                _commandDispatcher = commandDispatcher;
                _principal = principal;
                _processManagerFactory = processManagerFactory;
                _buildProcessId = buildProcessId ?? (correlationId => typeof(TProcess) + "-" + correlationId);
                _byCorrelationId = new Dictionary<Type, Func<object, string>>();
                _activeProcesses = new ConcurrentDictionary<string, TProcess>();
                _outbox = new Subject<CheckpointReached>();
            }

            public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>()
            {
                yield return async (message, ct) =>
                {
                    Func<object, string> getCorrelationId;
                    if(false == _byCorrelationId.TryGetValue(typeof(TMessage), out getCorrelationId))
                    {
                        return;
                    }

                    var correlationId = getCorrelationId(message);

                    TProcess process;
                    if(false == _activeProcesses.TryGetValue(correlationId, out process))
                    {
                        process = (TProcess) _processManagerFactory
                            .Build(typeof(TProcess), _buildProcessId(correlationId));

                        process.Commands.Select(command => Task.Run(() => DispatchCommand(process.Id, correlationId, command), ct))
                            .Subscribe(ct);

                        process.Events.OfType<DomainEventMessage<ProcessCompleted>>()
                            .Subscribe(e =>
                            {
                                TProcess _;
                                _activeProcesses.TryRemove(e.DomainEvent.ProcessId, out _);
                            });

                        _activeProcesses.TryAdd(process.Id, process);
                    }

                    process.Inbox.OnNext(message);
                };
            }

            public IEnumerator<Type> GetEnumerator()
            {
                return _byCorrelationId.Keys.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void CorrelateBy<TMessage>(
                Func<TMessage, string> getCorrelationId)
            {
                _byCorrelationId.Add(typeof(TMessage), message => getCorrelationId((TMessage)message));
            }

            [UsedImplicitly]
            private async Task DispatchCommand(string processId, string correlationId, object command)
            {
                Guard.EnsureNotNull(command, "command");

                await (Task)DispatchCommandMethodInfo.MakeGenericMethod(command.GetType())
                    .Invoke(null, new[]
                {
                    _commandDispatcher,
                    Guid.NewGuid(),
                    _principal,
                    command
                });

                _outbox.OnNext(new CheckpointReached
                {
                    CorrelationId = correlationId,
                    ProcessId = processId
                });
            }

            public IDisposable Subscribe(IObserver<CheckpointReached> observer)
            {
                return _outbox.Subscribe(observer);
            }
        }
    }
}