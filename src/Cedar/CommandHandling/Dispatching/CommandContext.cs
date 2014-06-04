namespace Cedar.CommandHandling.Dispatching
{
    using System;
    using System.Security.Claims;
    using System.Threading;

    public class CommandContext : ICommandContext
    {
        private readonly CancellationToken _cancellationToken;
        private readonly ClaimsPrincipal _user;
        private readonly Guid _commandId;

        public CommandContext(Guid commandId, CancellationToken cancellationToken, ClaimsPrincipal user)
        {
            _commandId = commandId;
            _cancellationToken = cancellationToken;
            _user = user;
        }

        public Guid CommandId
        {
            get { return _commandId; }
        }

        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
        }

        public ClaimsPrincipal User
        {
            get { return _user; }
        }
    }
}