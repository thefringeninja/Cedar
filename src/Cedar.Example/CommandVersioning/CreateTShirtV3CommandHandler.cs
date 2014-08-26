namespace Cedar.Example.CommandVersioning
{
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.Handlers;

    public class CreateTShirtV3CommandHandler : IHandler<CommandMessage<CreateTShirtV3>>
    {
        private readonly CreateTShirtV4CommandHandler _next;

        public CreateTShirtV3CommandHandler(CreateTShirtV4CommandHandler next)
        {
            this._next = next;
        }

        public Task Handle(CommandMessage<CreateTShirtV3> commandMessage, CancellationToken cancellationToken)
        {
            var command = new CreateTShirtV4
            {
                Name = commandMessage.Command.Name,
                Sizes = commandMessage.Command.Sizes,
                Colors = commandMessage.Command.Colors,
                BlankType = "Round"
            };
            return _next.Handle(
                new CommandMessage<CreateTShirtV4>(commandMessage.CommandId, commandMessage.RequstUser, command),
                cancellationToken);
        }
    }
}