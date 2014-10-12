namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Handlers;
    using Cedar.Internal;

    public static class ProcessHandlerModule
    {
        public static ProcessHandlerModule<TProcess> For<TProcess>(ICommandDispatcher dispatcher,
            Func<IProcessManagerRepository> repositoryFactory,
            Func<string, string> buildProcessId = null,
            string bucketId = null) where TProcess : IProcessManager
        {
            return new ProcessHandlerModule<TProcess>(dispatcher, repositoryFactory, buildProcessId, bucketId);
        }
    }

    public class ProcessHandlerModule<TProcess> : IHandlerResolver
        where TProcess : IProcessManager
    {
        private const string CommitIdNamespace = "73A18DFA-17C7-45A8-B57D-0148FDA3096A";
        private readonly ICommandDispatcher _dispatcher;
        private readonly Func<IProcessManagerRepository> _repositoryFactory;
        private readonly Func<string, string> _buildProcessId;
        private readonly string _bucketId;
        private readonly GenerateCommitId _buildCommitId;
        private readonly IDictionary<Type, Func<object, string>> _correlationIdLookup;
        private readonly IList<Pipe<DomainEventMessage<object>>> _pipes;

        private IHandlerResolver _inner;

        internal ProcessHandlerModule(
            ICommandDispatcher dispatcher,
            Func<IProcessManagerRepository> repositoryFactory,
            Func<string, string> buildProcessId = null,
            string bucketId = null)
        {
            _dispatcher = dispatcher;
            _repositoryFactory = repositoryFactory;
            _buildProcessId = buildProcessId ?? (correlationId => typeof(TProcess).Name + "-" + correlationId);
            _bucketId = bucketId;
            _pipes = new List<Pipe<DomainEventMessage<object>>>();

            var generator = new DeterministicGuidGenerator(Guid.Parse(CommitIdNamespace));

            _buildCommitId = (previousCommitId, processId, processVersion) => 
                generator.Create(previousCommitId.ToByteArray()
                    .Concat(Encoding.UTF8.GetBytes("-" + processId + "-")
                    .Concat(BitConverter.GetBytes(processVersion)))
                    .ToArray());

            _correlationIdLookup = new Dictionary<Type, Func<object, string>>();
        }

        public ProcessHandlerModule<TProcess> CorrelateBy<TMessage>(
            Func<DomainEventMessage<TMessage>, string> getCorrelationId)
        {
            _correlationIdLookup[typeof(TMessage)] = message => getCorrelationId((DomainEventMessage<TMessage>) message);

            return this;
        }

        public ProcessHandlerModule<TProcess> Pipe(Pipe<DomainEventMessage<object>> pipe)
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
            module.For<DomainEventMessage<TMessage>>()
                .Handle(async (message, ct) =>
                {
                    using (IProcessManagerRepository repository = _repositoryFactory())
                    {
                        string correlationId = _correlationIdLookup[typeof(TMessage)](message);

                        string processId = _buildProcessId(correlationId);

                        TProcess process = await repository.GetById<TProcess>(processId, _bucketId);

                        process.ApplyEvent(message.DomainEvent);

                        IEnumerable<Task> undispatched = process.GetUndispatchedCommands()
                            .Select(_dispatcher.Dispatch);

                        await Task.WhenAll(undispatched);

                        Guid commitId = _buildCommitId(message.Commit.CommitId, process.Id, process.Version);

                        await repository.Save(process, commitId, bucketId: _bucketId);
                    }
                });

            return module;
        }

        private delegate Guid GenerateCommitId(Guid incomingCommitId, string processId, int processVersion);
    }
}