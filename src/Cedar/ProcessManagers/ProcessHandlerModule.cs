namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Handlers;
    using Cedar.Internal;
    using Cedar.ProcessManagers.Persistence;

    public static class ProcessHandlerModule
    {
        public static ProcessHandlerModule<TProcess> For<TProcess>(
            IHandlerResolver commandDispatcher,
            IProcessManagerRepository repository,
            ClaimsPrincipal principal,
            Func<string, string> buildProcessId = null,
            string bucketId = null) where TProcess : IProcessManager
        {
            return new ProcessHandlerModule<TProcess>(commandDispatcher, repository, principal, buildProcessId, bucketId);
        }
    }

    public class ProcessHandlerModule<TProcess> : IHandlerResolver
        where TProcess : IProcessManager
    {
        // ReSharper disable StaticFieldInGenericType
        private static readonly MethodInfo DispatchCommandMethodInfo = typeof(HandlerModulesDispatchCommand)
            .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.Public);
        // ReSharper restore StaticFieldInGenericType

        private const string CommitIdNamespace = "73A18DFA-17C7-45A8-B57D-0148FDA3096A";
        
        private readonly IHandlerResolver _commandDispatcher;
        private readonly IProcessManagerRepository _repository;
        private readonly ClaimsPrincipal _principal;
        private readonly Func<string, string> _buildProcessId;
        private readonly string _bucketId;
        private readonly GenerateCommitId _buildCommitId;
        private readonly IDictionary<Type, Func<object, string>> _correlationIdLookup;
        private readonly IList<Pipe<object>> _pipes;

        private IHandlerResolver _inner;

        internal ProcessHandlerModule(
            IHandlerResolver commandDispatcher,
            IProcessManagerRepository repository, 
            ClaimsPrincipal principal,
            Func<string, string> buildProcessId = null, 
            string bucketId = null)
        {
            _commandDispatcher = commandDispatcher;
            _repository = repository;
            _principal = principal;
            _buildProcessId = buildProcessId ?? (correlationId => typeof(TProcess).Name + "-" + correlationId);
            _bucketId = bucketId;
            _pipes = new List<Pipe<object>>();

            var generator = new DeterministicGuidGenerator(Guid.Parse(CommitIdNamespace));

            _buildCommitId = (message, processId, processVersion) =>
                generator.Create(Encoding.UTF8.GetBytes("-" + processId + "-")
                    .Concat(BitConverter.GetBytes(processVersion)).ToArray());

            _correlationIdLookup = new Dictionary<Type, Func<object, string>>();
        }

        public ProcessHandlerModule<TProcess> CorrelateBy<TMessage>(
            Func<TMessage, string> getCorrelationId)
        {
            _correlationIdLookup[typeof(TMessage)] = message => getCorrelationId((TMessage) message);

            return this;
        }

        public ProcessHandlerModule<TProcess> Pipe(Pipe<object> pipe)
        {
            _pipes.Add(pipe);

            return this;
        }

        public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>()
        {
            return (_inner ?? (_inner = BuildHandlerResolver())).GetHandlersFor<TMessage>();
        }

        private IHandlerResolver BuildHandlerResolver()
        {
            return _correlationIdLookup.Keys
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
        {
            module.For<TMessage>()
                .Handle(async (message, ct) =>
                {
                    string correlationId = _correlationIdLookup[typeof(TMessage)](message);

                    string processId = _buildProcessId(correlationId);

                    TProcess process = await _repository.GetById<TProcess>(_bucketId, processId, Int32.MaxValue, ct);

                    process.ApplyEvent(message);

                    IEnumerable<Task> undispatched = process.GetUndispatchedCommands()
                        .Select(DispatchCommand);

                    await Task.WhenAll(undispatched);

                    Guid commitId = _buildCommitId(message, process.Id, process.Version);

                    await _repository.Save(_bucketId, process, null, ct);
                });

            return module;
        }

        private Task DispatchCommand(object command)
        {
            Guard.EnsureNotNull(command, "command");

            return (Task)DispatchCommandMethodInfo.MakeGenericMethod(command.GetType())
                .Invoke(null, new[]
                {
                    _commandDispatcher,
                    Guid.NewGuid(),
                    _principal,
                    command
                });
        }

        private delegate Guid GenerateCommitId(object message, string processId, int processVersion);
    }
}