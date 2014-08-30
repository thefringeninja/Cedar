namespace Cedar.Commands
{
    using System;
    using System.Security.Claims;

    public class CommandMessage<TCommand>
    {
        private readonly TCommand _command;
        private readonly Guid _commandId;
        private readonly ClaimsPrincipal _requestUser;

        public CommandMessage(Guid commandId, ClaimsPrincipal requestUser, TCommand command)
        {
            _commandId = commandId;
            _requestUser = requestUser;
            _command = command;
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
    }
}
