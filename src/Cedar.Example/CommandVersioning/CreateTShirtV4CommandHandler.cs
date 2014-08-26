namespace Cedar.Example.CommandVersioning
{
    using System.Threading.Tasks;
    using Cedar.CommandHandling;

    public class CreateTShirtV4CommandHandler : ICommandHandler<CreateTShirtV4>
    {
        private readonly IEventPublisher _publisher;

        public CreateTShirtV4CommandHandler(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        public Task Handle(ICommandContext context, CreateTShirtV4 command)
        {
            _publisher.Publish(new TShirtCreatedV4
            {
                BlankType = command.BlankType,
                Name = command.Name,
                Colors = command.Colors,
                Sizes = command.Sizes
            });

            return Task.FromResult(true);
        }
    }
}