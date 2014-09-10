namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Cedar.Internal;

    public class ProcessHandlerModule<TProcess> : IHandlerResolver
        where TProcess : IProcessManager
    {
        private const string NAMESPACE = "73A18DFA-17C7-45A8-B57D-0148FDA3096A";
        private readonly ICommandDispatcher _dispatcher;
        private readonly Func<IProcessManagerRepository> _repositoryFactory;
        private readonly Func<Guid, string> _buildId;
        private readonly string _bucketId;
        private readonly HandlerModule _inner;
        private readonly Func<Guid, string, Guid> _buildCommitId;
        public ProcessHandlerModule(ICommandDispatcher dispatcher, Func<IProcessManagerRepository> repositoryFactory, Func<Guid, string> buildId = null, string bucketId = null)
        {
            _dispatcher = dispatcher;
            _repositoryFactory = repositoryFactory;
            _buildId = buildId ?? (correlationId => typeof(TProcess).Name + "-" + correlationId);
            _bucketId = bucketId;

            _inner = new HandlerModule();
            var generator = new DeterministicGuidGenerator(Guid.Parse(NAMESPACE));
            
            _buildCommitId = (commitId, processId) => generator.Create(commitId + "-" + processId);
        }

        public void When<TMessage>(params Pipe<DomainEventMessage<TMessage>>[] pipeline)
        {
            pipeline.Aggregate(_inner.For<DomainEventMessage<TMessage>>(), (builder, pipe) => builder.Pipe(pipe),
                builder => builder.Handle(HandleMessage));
        }

        private async Task HandleMessage<TMessage>(DomainEventMessage<TMessage> message, CancellationToken ct)
        {
            using (var repository = _repositoryFactory())
            {
                var correlationId = message.CorrelationId;

                if (false == correlationId.HasValue) return;

                var processId = _buildId(correlationId.Value);

                var process = await repository.GetById<TProcess>(processId, _bucketId);

                process.ApplyEvent(message.DomainEvent);

                var undispatched = process.GetUndispatchedCommands()
                    .Select(_dispatcher.Dispatch);

                await Task.WhenAll(undispatched);

                var commitId = _buildCommitId(message.Commit.CommitId, processId);

                await repository.Save(process, commitId, bucketId: _bucketId);
            }
        }

        public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>()
        {
            return _inner.GetHandlersFor<TMessage>();
        }
    }
}