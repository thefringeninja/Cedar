namespace Cedar.Commands
{
    using System;
    using System.Security.Claims;

    public class CommandMessage<TCommand>
    {
        private readonly TCommand _command;
        private readonly Guid _commandId;
        private readonly ClaimsPrincipal _requstUser;

        public CommandMessage(Guid commandId, ClaimsPrincipal requstUser, TCommand command)
        {
            _commandId = commandId;
            _requstUser = requstUser;
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

        public ClaimsPrincipal RequstUser
        {
            get { return _requstUser; }
        }
    }
}