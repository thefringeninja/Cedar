namespace Cedar.CommandHandling
{
    public interface ICommandHandlerResolver
    {
        ICommandHandler<T> Resolve<T>()
            where T : class;
    }
}