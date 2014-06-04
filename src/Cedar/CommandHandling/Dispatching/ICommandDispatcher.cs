namespace Cedar.CommandHandling.Dispatching
{
    using System.Threading.Tasks;

    public interface ICommandDispatcher
    {
        Task Dispatch(ICommandContext commandContext, object command);
    }
}