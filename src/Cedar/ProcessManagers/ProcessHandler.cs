namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Handlers;
    using Cedar.ProcessManagers.Messages;
    using Cedar.ProcessManagers.Persistence;
    using CuttingEdge.Conditions;

    public static class ProcessHandler
    {
        public static ProcessHandler<TProcess, TCheckpoint> For<TProcess, TCheckpoint>(
            ICommandHandlerResolver commandDispatcher, //TODO Process manager should use HttpClient to invoke commands like a user
            ClaimsPrincipal principal,
            IProcessManagerCheckpointRepository<TCheckpoint> checkpointRepository,
            IProcessManagerFactory processManagerFactory = null,
            ProcessHandler<TProcess, TCheckpoint>.BuildProcessManagerId buildProcessId = null) 
            where TProcess : IProcessManager 
            where TCheckpoint : IComparable<string>
        {
            return new ProcessHandler<TProcess, TCheckpoint>(commandDispatcher, principal, checkpointRepository, processManagerFactory, buildProcessId);
        }
    }

    public class ProcessHandler<TProcess,TCheckpoint> : IHandlerResolver where TProcess : IProcessManager where TCheckpoint : IComparable<string>
    {
        public static BuildProcessManagerId DefaultBuildProcessManagerId = correlationId => typeof(TProcess).Name + "-" + correlationId;

        public delegate string BuildProcessManagerId(string correlationId);

        private readonly IList<Pipe<object>> _pipes;
        private readonly ProcessManagerDispatcher _dispatcher;

        internal ProcessHandler(
            ICommandHandlerResolver commandDispatcher,
            ClaimsPrincipal principal,
            IProcessManagerCheckpointRepository<TCheckpoint> checkpointRepository,
            IProcessManagerFactory processManagerFactory = null,
            BuildProcessManagerId buildProcessId = null)
        {
            Condition.Requires(commandDispatcher, "commandDispatcher").IsNotNull();
            Condition.Requires(principal, "principal").IsNotNull();
            Condition.Requires(checkpointRepository, "checkpointRepository").IsNotNull();

            _pipes = new List<Pipe<object>>();
            _dispatcher = new ProcessManagerDispatcher(commandDispatcher, principal, checkpointRepository, processManagerFactory, buildProcessId);
            CorrelateBy<ProcessCompleted>(message => message.DomainEvent.CorrelationId)
                .CorrelateBy<CheckpointReached>(message => message.DomainEvent.CorrelationId);
        }

        public ProcessHandler<TProcess, TCheckpoint> CorrelateBy<TMessage>(
            Func<DomainEventMessage<TMessage>, string> getCorrelationId) where TMessage : class
        {
            _dispatcher.CorrelateBy(getCorrelationId);

            return this;
        }

        public ProcessHandler<TProcess, TCheckpoint> Pipe(Pipe<object> pipe)
        {
            _pipes.Add(pipe);

            return this;
        }

        public IHandlerResolver BuildHandlerResolver()
        {
            return _dispatcher
                .Aggregate(new HandlerModule(), HandleMessageType);
        }

        private HandlerModule HandleMessageType(HandlerModule module, Type messageType)
        {
            return (HandlerModule)GetType()
                .GetMethod("BuildHandler", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(messageType)
                .Invoke(this, new object[] { module });
        }

        [UsedImplicitly]
        private HandlerModule BuildHandler<TMessage>(HandlerModule module)
            where TMessage : class
        {
            _pipes.Select(pipe => Delegate.CreateDelegate(typeof(Pipe<TMessage>), pipe.Method) as Pipe<TMessage>)
                .Aggregate(module.For<TMessage>(), (builder, pipe) => builder.Pipe(pipe))
                .Handle(_dispatcher.Dispatch);

            return module;
        }

        public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>() where TMessage : class
        {
            return _dispatcher.GetHandlersFor<TMessage>();
        }

        class CheckpointedProcess
        {
            public readonly TCheckpoint Checkpoint;
            public readonly TProcess Process;

            public CheckpointedProcess(TProcess process, TCheckpoint checkpoint)
            {
                Process = process;
                Checkpoint = checkpoint;
            }
        }

        class ProcessManagerDispatcher : IHandlerResolver, IEnumerable<Type>
        {
            private readonly ICommandHandlerResolver _commandDispatcher;
            private readonly ClaimsPrincipal _principal;
            private readonly IProcessManagerCheckpointRepository<TCheckpoint> _checkpointRepository;
            private readonly IProcessManagerFactory _processManagerFactory;
            private readonly BuildProcessManagerId _buildProcessId;
            private readonly IDictionary<Type, Func<object, string>> _byCorrelationId;
            private readonly ConcurrentDictionary<string, CheckpointedProcess> _activeProcesses;

            public ProcessManagerDispatcher(
                ICommandHandlerResolver commandDispatcher,
                ClaimsPrincipal principal,
                IProcessManagerCheckpointRepository<TCheckpoint> checkpointRepository,
                IProcessManagerFactory processManagerFactory = null,
                BuildProcessManagerId buildProcessId = null)
            {
                _commandDispatcher = commandDispatcher;
                _principal = principal;
                _checkpointRepository = checkpointRepository;
                _processManagerFactory = processManagerFactory ?? new DefaultProcessManagerFactory();
                _buildProcessId = buildProcessId ?? DefaultBuildProcessManagerId;
                _byCorrelationId = new Dictionary<Type, Func<object, string>>();
                _activeProcesses = new ConcurrentDictionary<string, CheckpointedProcess>();
            }

            public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>() where TMessage : class
            {
                if(false == typeof(DomainEventMessage).IsAssignableFrom(typeof(TMessage))
                    || false == _byCorrelationId.ContainsKey(typeof(TMessage)))
                {
                    yield break;
                }
                yield return async (message, ct) =>
                {
                    Func<object, string> getCorrelationId;
                    if(false == _byCorrelationId.TryGetValue(typeof(TMessage), out getCorrelationId))
                    {
                        return;
                    }

                    var domainEventMessage = (message as DomainEventMessage);

                    var correlationId = getCorrelationId(message);

                    var checkpointedProcess = await GetProcess(correlationId, ct);

                    var process = checkpointedProcess.Process;
                    var checkpoint = checkpointedProcess.Checkpoint;

                    process.Inbox.OnNext(domainEventMessage);

                    if (checkpoint.CompareTo(domainEventMessage.CheckpointToken) >= 0)
                    {
                        return;
                    }

                    var commands = process.Commands.ToList();

                    if(false == commands.Any())
                    {
                        return;
                    }

                    await Task.WhenAll(commands.Select(command => DispatchCommand(process, command, ct)));

                    await _checkpointRepository.SaveCheckpointToken(process, domainEventMessage.CheckpointToken, ct);
                };
            }

            private async Task<CheckpointedProcess> GetProcess(string correlationId, CancellationToken ct)
            {
                CheckpointedProcess checkpointedProcess;
                if(false == _activeProcesses.TryGetValue(correlationId, out checkpointedProcess))
                {
                    var process = (TProcess) _processManagerFactory
                        .Build(typeof(TProcess), _buildProcessId(correlationId), correlationId);

                    var checkpoint = await _checkpointRepository.GetCheckpoint(process.Id, ct);

                    process.Events.OfType<ProcessCompleted>()
                        .Subscribe(async e =>
                        {
                            CheckpointedProcess _;
                            _activeProcesses.TryRemove(e.ProcessId, out _);
                            await _checkpointRepository.MarkProcessCompleted(e, ct);
                            process.Dispose();
                        });

                    checkpointedProcess = new CheckpointedProcess(process, checkpoint);

                    _activeProcesses.TryAdd(process.Id, checkpointedProcess);
                }
                return checkpointedProcess;
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
                Func<DomainEventMessage<TMessage>, string> getCorrelationId) where TMessage : class
            {
                _byCorrelationId.Add(typeof(DomainEventMessage<TMessage>), message => getCorrelationId((DomainEventMessage<TMessage>)message));
            }

            [UsedImplicitly]
            private async Task DispatchCommand(TProcess process, object command, CancellationToken ct)
            {
                await (Task)CommandController.DispatchCommandMethodInfo.MakeGenericMethod(command.GetType())
                    .Invoke(null, new[]
                {
                    _commandDispatcher,
                    Guid.NewGuid(),
                    _principal,
                    command,
                    ct
                });
            }
        }
    }
}