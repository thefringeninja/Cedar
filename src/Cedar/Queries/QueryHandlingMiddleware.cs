namespace Cedar.Queries
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.ExceptionModels;
    using Cedar.Serialization.Client;
    using Cedar.TypeResolution;
    using Microsoft.Owin;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;


    public static class QueryHandlingMiddleware
    {
        private static readonly MethodInfo DispatchQueryMethodInfo = typeof(HandlerModulesDispatchQuery)
            .GetMethod("DispatchQuery", BindingFlags.Static | BindingFlags.Public);

        private static readonly MethodInfo WidenTaskResultMethodInfo =
            typeof (QueryHandlingMiddleware).GetMethod("WidenTaskResult", BindingFlags.Static | BindingFlags.NonPublic);

        public static MidFunc HandleQueries(HandlerSettings options,
            Func<IDictionary<string, object>, Type> getInputType = null,
            Func<IDictionary<string, object>, Type> getOutputType = null, 
            string queryPath = "/query")
        {
            Guard.EnsureNotNull(options, "options");

            var acceptableMethods = new[] {"GET", "POST"};

            return next => env =>
            {
                var context = new OwinContext(env);

                if (!context.Request.Path.StartsWithSegments(new PathString(queryPath)))
                {
                    return next(env);
                }

                if (!acceptableMethods.Contains(context.Request.Method))
                {
                    return next(env);
                }

                return BuildHandlerCall()
                    .ExecuteWithExceptionHandling(context, options);
            };
        }

        private static Func<IOwinContext, HandlerSettings, Task> BuildHandlerCall()
        {
            return (context, options) => HandleQuery(
                context,
                Guid.NewGuid(),
                options);
        }

        private static async Task HandleQuery(IOwinContext context, Guid queryId, HandlerSettings options)
        {
            var request = new CedarRequest(context);
            
            var inputType = options.RequestTypeResolver.ResolveInputType(request);

            if (inputType == null)
            {
                throw new HttpStatusException("Unable to find the query type", HttpStatusCode.BadRequest, new NotSupportedException());
            }

            var outputType = options.RequestTypeResolver.ResolveOutputType(request);

            object input;
            using (var streamReader = new StreamReader(context.Request.Body))
            {
                input = options.Serializer.Deserialize(streamReader, inputType) 
                    ?? Activator.CreateInstance(inputType);
            }
            var user = (context.Request.User as ClaimsPrincipal) ?? new ClaimsPrincipal(new ClaimsIdentity());
            var dispatchQuery = DispatchQueryMethodInfo.MakeGenericMethod(inputType, outputType);

            var task = dispatchQuery.Invoke(null, new[]
            {
                options.HandlerModules, queryId, user, input, context.Request.CallCancelled
            });
            
            var result = await (Task<object>) WidenTaskResultMethodInfo
                .MakeGenericMethod(outputType)
                .Invoke(null, new[] {task});

            var body = options.Serializer.Serialize(result);
            await context.Response.WriteAsync(body);
            context.Response.StatusCode = 200;
            context.Response.ReasonPhrase = "OK";
        }

        [UsedImplicitly]
        private static async Task<object> WidenTaskResult<TResult>(Task<TResult> task)
        {
            return await task;
        }
    }
}
