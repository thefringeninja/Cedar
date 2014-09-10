namespace Cedar.Queries
{
    using System.Threading.Tasks;

    public interface IQueryHandler<TInput, TOutput>
    {
        Task<TOutput> PerformQuery(TInput input);
    }
}