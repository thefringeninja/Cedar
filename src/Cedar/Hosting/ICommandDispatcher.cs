namespace CED.Hosting
{
    using System.Threading.Tasks;
    using CED.Domain;

    public interface ICommandDispatcher
    {
        Task Dispatch(ICommandContext commandContext, object command);
    }
}