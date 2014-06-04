namespace Cedar.Hosting
{
    using Cedar.Domain;

    public interface ICommandHandlerResolver
    {
        ICommandHandler<T> Resolve<T>()
            where T : class;
    }
}