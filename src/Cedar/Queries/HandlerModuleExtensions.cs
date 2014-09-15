namespace Cedar.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Handlers;

    public static class HandlerModulesDispatchQuery
    {
        [UsedImplicitly]
        public static async Task<TOutput> DispatchQuery<TInput, TOutput>(
            this IEnumerable<IHandlerResolver> handlerModules,
            Guid queryId,
            ClaimsPrincipal requstUser,
            TInput query,
            CancellationToken cancellationToken)
        {
            Guard.EnsureNotNull(handlerModules, "handlerModules");
            Guard.EnsureNotNull(requstUser, "requstUser");
            Guard.EnsureNotNull(query, "query");

            var queryMessage = new QueryMessage<TInput, TOutput>(queryId, requstUser, query);
            await handlerModules.DispatchSingle(queryMessage, cancellationToken);

            return await queryMessage.Source.Task;
        }
    }

    public static class HandlerModuleDispatchQuery
    {
        // This in a seperate class because of a generic limitation in reflection
        // http://blogs.msdn.com/b/yirutang/archive/2005/09/14/466280.aspx

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="handlerModule"></param>
        /// <param name="queryId"></param>
        /// <param name="requstUser"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public static async Task<TOutput> DispatchQuery<TInput, TOutput>(
            this IHandlerResolver handlerModule,
            Guid queryId,
            ClaimsPrincipal requstUser,
            TInput query,
            CancellationToken cancellationToken)
        {
            Guard.EnsureNotNull(handlerModule, "handlerModules");
            Guard.EnsureNotNull(requstUser, "requstUser");
            Guard.EnsureNotNull(query, "query");

            var queryMessage = new QueryMessage<TInput, TOutput>(queryId, requstUser, query);
            await handlerModule.DispatchSingle(queryMessage, cancellationToken);

            return await queryMessage.Source.Task;
        }

        public static ICreateHandlerBuilder HandleQuery<TInput, TOutput>(
            this IHandlerBuilder<QueryMessage<TInput, TOutput>> handler, Func<TInput, CancellationToken, Task<TOutput>> query)
        {
            return handler.Handle(async (message, ct) =>
            {
                try
                {
                    var result = await query(message.Input, ct);
                    
                    if (ct.IsCancellationRequested)
                    {
                        message.Source.TrySetCanceled();
                    }
                    else
                    {
                        message.Source.TrySetResult(result);
                    }
                }
                catch (Exception ex)
                {
                    message.Source.TrySetException(ex);
                }
            });
        }
    }
}