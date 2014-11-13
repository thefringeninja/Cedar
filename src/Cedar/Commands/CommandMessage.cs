namespace Cedar.Commands
{
    using System;
    using System.Security.Claims;

    public class CommandMessage
    {
        private readonly Guid _commandId;
        private readonly ClaimsPrincipal _requestUser;
        private readonly object _command;

        public CommandMessage(
            Guid commandId,
            ClaimsPrincipal requestUser,
            object command)
        {
            _commandId = commandId;
            _requestUser = requestUser;
            _command = command;
        }

        public Guid CommandId
        {
            get { return _commandId; }
        }

        public ClaimsPrincipal RequestUser
        {
            get { return _requestUser; }
        }

        public object Command
        {
            get { return _command; }
        }

        public override string ToString()
        {
            return Command.ToString();
        }
    }

    public class CommandMessage<TCommand> : CommandMessage
    {
        private readonly TCommand _command;

        public CommandMessage(
            Guid commandId,
            ClaimsPrincipal requestUser,
            TCommand command) : base(commandId, requestUser, command)
        {
            _command = command;
        }

        public new TCommand Command
        {
            get { return _command; }
        }
    }
}