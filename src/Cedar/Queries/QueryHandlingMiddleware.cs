namespace Cedar.Queries
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Microsoft.Owin;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;


    public static class QueryHandlingMiddleware
    {
        private static readonly MethodInfo DispatchQueryMethodInfo = typeof(HandlerModulesDispatchQuery)
            .GetMethod("DispatchQuery", BindingFlags.Static | BindingFlags.Public);

        public static MidFunc HandleQueries(HandlerSettings options, 
            Func<IOwinContext, Task<Type>> getInputType = null, 
            Func<IOwinContext, Task<Type>> getOutputType = null, 
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

                try
                {
                    return HandleQuery(
                        context, 
                        Guid.NewGuid(), 
                        options, 
                        getInputType ?? QueryTypeMapping.InputTypeFromPathSegment(options, queryPath), 
                        getOutputType ?? QueryTypeMapping.OutputTypeFromAcceptHeader(options));
                }
                catch (InvalidOperationException ex)
                {
                    return context.HandleBadRequest(ex, options);
                }
                catch (Exception ex)
                {
                    return context.HandleInternalServerError(ex, options);
                }

            };
        }

        private static async Task HandleQuery(IOwinContext context, Guid queryId, HandlerSettings options, Func<IOwinContext, Task<Type>> getInputType,
            Func<IOwinContext, Task<Type>> getOutputType)
        {
            var inputType = await getInputType(context);

            if (inputType == null)
            {
                return;
            }

            var outputType = await getOutputType(context);

            if (outputType == null)
            {
                return;
            }

            object input;
            using (var streamReader = new StreamReader(context.Request.Body))
            {
                input = options.Serializer.Deserialize(streamReader, inputType);
            }
            var user = (context.Request.User as ClaimsPrincipal) ?? new ClaimsPrincipal(new ClaimsIdentity());
            var dispatchQuery = DispatchQueryMethodInfo.MakeGenericMethod(input.GetType(), outputType);
            await (Task)dispatchQuery.Invoke(null, new[]
            {
                options.HandlerModules, queryId, user, input, context.Request.CallCancelled
            });
            context.Response.StatusCode = 200;
            context.Response.ReasonPhrase = "OK";
        }
    }
}
