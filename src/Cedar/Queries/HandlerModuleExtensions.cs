namespace Cedar.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Handlers;
    using CuttingEdge.Conditions;

    public static class HandlerModulesDispatchQuery
    {
        [UsedImplicitly]
        public static async Task<TOutput> DispatchQuery<TInput, TOutput>(
            this IEnumerable<IHandlerResolver> handlerModules,
            Guid queryId,
            ClaimsPrincipal requstUser,
            TInput query,
            CancellationToken cancellationToken)
            where TInput : class
        {
            Condition.Requires(handlerModules, "handlermodules").IsNotNull();
            Condition.Requires(requstUser, "requstUser").IsNotNull();
            Condition.Requires(query, "query").IsNotNull();

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
            Condition.Requires(handlerModule, "handlerModule").IsNotNull();
            Condition.Requires(requstUser, "requstUser").IsNotNull();

            var queryMessage = new QueryMessage<TInput, TOutput>(queryId, requstUser, query);
            await handlerModule.DispatchSingle(queryMessage, cancellationToken);

            return await queryMessage.Source.Task;
        }

        public static void HandleQuery<TInput, TOutput>(
            this IHandlerBuilder<QueryMessage<TInput, TOutput>> handler, Func<TInput, CancellationToken, Task<TOutput>> query)
        {
            handler.Handle(async (message, ct) =>
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