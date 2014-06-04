namespace Cedar.Hosting
{
    using System.Threading.Tasks;
    using Cedar.Domain;

    public interface ICommandDispatcher
    {
        Task Dispatch(ICommandContext commandContext, object command);
    }
}