namespace CED.Hosting
{
    using CED.Domain;

    public interface ICommandHandlerResolver
    {
        ICommandHandler<T> Resolve<T>()
            where T : class;
    }
}