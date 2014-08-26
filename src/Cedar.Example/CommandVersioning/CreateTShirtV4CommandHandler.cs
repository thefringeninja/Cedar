namespace Cedar.Example.CommandVersioning
{
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.Handlers;

    public class CreateTShirtV4CommandHandler : IHandler<CommandMessage<CreateTShirtV4>>
    {
        private readonly IEventPublisher _publisher;

        public CreateTShirtV4CommandHandler(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        public Task Handle(CommandMessage<CreateTShirtV4> commandMessage, CancellationToken cancellationToken)
        {
            _publisher.Publish(new TShirtCreatedV4
            {
                BlankType = commandMessage.Command.BlankType,
                Name = commandMessage.Command.Name,
                Colors = commandMessage.Command.Colors,
                Sizes = commandMessage.Command.Sizes
            });

            return Task.FromResult(true);
        }
    }
}