namespace Cedar.Commands
{
    using System;
    using System.Security.Claims;

    public class CommandMessage<TCommand>
    {
        private readonly TCommand _command;
        private readonly Guid _commandId;
        private readonly Guid _correlationId;
        private readonly ClaimsPrincipal _requestUser;

        public CommandMessage(Guid commandId, ClaimsPrincipal requestUser, TCommand command, Guid? correlationId = default (Guid?))
        {
            _commandId = commandId;
            _requestUser = requestUser;
            _command = command;
            _correlationId = correlationId ?? Guid.NewGuid();
        }

        public TCommand Command
        {
            get { return _command; }
        }

        public Guid CommandId
        {
            get { return _commandId; }
        }

        public ClaimsPrincipal RequestUser
        {
            get { return _requestUser; }
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public override string ToString()
        {
            return Command.ToString();
        }
    }
}
