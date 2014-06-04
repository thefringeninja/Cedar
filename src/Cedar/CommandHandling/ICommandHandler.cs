namespace Cedar.CommandHandling
{
    using System.Threading.Tasks;
    using Cedar.CommandHandling.Dispatching;

    public interface ICommandHandler<in T>
        where T : class
    {
        Task Handle(ICommandContext context, T command);
    }
}