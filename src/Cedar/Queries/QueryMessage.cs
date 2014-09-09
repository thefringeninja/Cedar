namespace Cedar.Queries
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class QueryMessage<TInput, TOutput>
    {
        private readonly Guid _queryId;
        private readonly ClaimsPrincipal _requestUser;
        private readonly TInput _input;
        private readonly TaskCompletionSource<TOutput> _source;

        public QueryMessage(Guid queryId, ClaimsPrincipal requestUser, TInput input)
        {
            _queryId = queryId;
            _requestUser = requestUser;
            _input = input;
            _source = new TaskCompletionSource<TOutput>();
        }

        public Guid QueryId
        {
            get { return _queryId; }
        }

        public ClaimsPrincipal RequestUser
        {
            get { return _requestUser; }
        }

        public TInput Input
        {
            get { return _input; }
        }

        public TaskCompletionSource<TOutput> Source
        {
            get { return _source; }
        }

        public override string ToString()
        {
            return "Query: " + _input + ", on behalf of " + _requestUser.Identity;
        }
    }
}