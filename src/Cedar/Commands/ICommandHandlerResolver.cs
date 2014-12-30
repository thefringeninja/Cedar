namespace Cedar.Commands
{
    using Cedar.Handlers;

    public interface ICommandHandlerResolver
    {
        Handler<CommandMessage<TCommand>> Resolve<TCommand>() where TCommand : class;
    }
}