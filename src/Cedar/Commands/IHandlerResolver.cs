namespace Cedar.Commands
{
    using Cedar.Handlers;

    public interface ICommandHandlerResolver
    {
        Handler<TCommand> Resolve<TCommand>() where TCommand : class;
    }
}