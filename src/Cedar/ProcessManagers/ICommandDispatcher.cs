namespace Cedar.ProcessManagers
{
    using System.Threading.Tasks;

    public interface ICommandDispatcher
    {
        Task Dispatch(object command);
    }
}