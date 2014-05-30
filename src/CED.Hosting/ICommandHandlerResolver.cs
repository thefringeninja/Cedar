namespace CED.Hosting
{
    using CED.Framework.Domain;

    public interface ICommandHandlerResolver
    {
        ICommandHandler<T> Resolve<T>()
            where T : class;
    }
}