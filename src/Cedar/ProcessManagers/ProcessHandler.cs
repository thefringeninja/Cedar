namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Handlers;

    public static class ProcessHandler
    {
        public static ProcessHandler<TProcess> For<TProcess>(
            IHandlerResolver commandDispatcher,
            ClaimsPrincipal principal,
            ProcessHandler<TProcess>.GetProcess getProcess,
            ProcessHandler<TProcess>.SaveProcess saveProcess,
            Func<string, string> buildProcessId = null,
            string bucketId = null) where TProcess : IProcessManager
        {
            return new ProcessHandler<TProcess>(commandDispatcher, principal, getProcess, saveProcess, buildProcessId, bucketId);
        }
    }

    public class ProcessHandler<TProcess> : IHandlerResolver
        where TProcess : IProcessManager
    {
        // ReSharper disable StaticFieldInGenericType
        private static readonly MethodInfo DispatchCommandMethodInfo = typeof(HandlerModulesDispatchCommand)
            .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.Public);
        // ReSharper restore StaticFieldInGenericType

        private readonly IHandlerResolver _commandDispatcher;
        private readonly ClaimsPrincipal _principal;
        private readonly Func<string, string> _buildProcessId;
        private readonly string _bucketId;
        private readonly IDictionary<Type, Func<object, string>> _correlationIdLookup;
        private readonly IList<Pipe<object>> _pipes;
        private readonly GetProcess _getProcess;
        private readonly SaveProcess _saveProcess;

        private IHandlerResolver _inner;

        internal ProcessHandler(
            IHandlerResolver commandDispatcher, 
            ClaimsPrincipal principal,
            GetProcess getProcess, 
            SaveProcess saveProcess,
            Func<string, string> buildProcessId = null, 
            string bucketId = null)
        {
            _commandDispatcher = commandDispatcher;
            _principal = principal;
            _buildProcessId = buildProcessId ?? (correlationId => typeof(TProcess).Name + "-" + correlationId);
            _bucketId = bucketId;
            _getProcess = getProcess;
            _saveProcess = saveProcess;
            _pipes = new List<Pipe<object>>();

            _correlationIdLookup = new Dictionary<Type, Func<object, string>>();
        }

        public ProcessHandler<TProcess> CorrelateBy<TMessage>(
            Func<TMessage, string> getCorrelationId)
        {
            _correlationIdLookup[typeof(TMessage)] = message => getCorrelationId((TMessage) message);

            return this;
        }

        public ProcessHandler<TProcess> Pipe(Pipe<object> pipe)
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

                    TProcess process = await _getProcess(_bucketId, processId, ct);

                    process.ApplyEvent(message);

                    IEnumerable<Task> undispatched = process.GetUndispatchedCommands()
                        .Select(DispatchCommand);

                    await Task.WhenAll(undispatched);

                    await _saveProcess(_bucketId, process, ct);
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

        public delegate Task<TProcess> GetProcess(string bucketId, string processId, CancellationToken token);

        public delegate Task SaveProcess(string bucketId, TProcess process, CancellationToken token);
    }
}