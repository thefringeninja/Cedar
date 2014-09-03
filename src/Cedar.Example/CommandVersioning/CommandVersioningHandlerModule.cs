namespace Cedar.Example.CommandVersioning
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.Handlers;

    public class CommandVersioningHandlerModule : HandlerModule
    {
        public CommandVersioningHandlerModule(IEventPublisher publisher)
        {
            For<CommandMessage<CreateTShirtV2>>()
                .Handle((_, __) => { throw new NotSupportedException(); });

            For<CommandMessage<CreateTShirtV3>>()
                .Handle((commandMessage, ct) =>
                {
                    var command = new CreateTShirtV4
                    {
                        Name = commandMessage.Command.Name,
                        Sizes = commandMessage.Command.Sizes,
                        Colors = commandMessage.Command.Colors,
                        BlankType = "Round"
                    };
                    var upconvertedCommand = new CommandMessage<CreateTShirtV4>(
                        commandMessage.CommandId,
                        commandMessage.RequestUser, 
                        command);
                    return this.DispatchSingle(upconvertedCommand, ct);
                });

            For<CommandMessage<CreateTShirtV4>>()
                .Handle((commandMessage, ct) =>
                {
                    publisher.Publish(new TShirtCreatedV4
                    {
                        BlankType = commandMessage.Command.BlankType,
                        Name = commandMessage.Command.Name,
                        Colors = commandMessage.Command.Colors,
                        Sizes = commandMessage.Command.Sizes
                    });
                    return Task.FromResult(true);
                });
        }
    }
}