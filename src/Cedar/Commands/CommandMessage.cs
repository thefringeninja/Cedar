namespace Cedar.Commands
{
    using System;
    using System.Security.Claims;

    public class CommandMessage<TCommand>
    {
        private readonly TCommand _command;
        private readonly Guid _commandId;
        private readonly ClaimsPrincipal _user;

        public CommandMessage(
            Guid commandId,
            ClaimsPrincipal user,
            TCommand command)
        {
            _commandId = commandId;
            _user = user;
            _command = command;
        }

        public Guid CommandId
        {
            get { return _commandId; }
        }

        public ClaimsPrincipal User
        {
            get { return _user; }
        }

        public TCommand Command
        {
            get { return _command; }
        }
    }
}