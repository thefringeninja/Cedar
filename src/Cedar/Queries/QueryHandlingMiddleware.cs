namespace Cedar.Queries
{
    using System;
    using System.Collections.Generic;
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

                try
                {
                    return HandleQuery(
                        context,
                        Guid.NewGuid(),
                        options,
                        getInputType ?? QueryTypeMapping.InputTypeFromPathSegment(options, queryPath),
                        getOutputType ?? QueryTypeMapping.OutputTypeFromAcceptHeader(options));
                }
                catch (HttpStatusException ex)
                {
                    return context.HandleHttpStatusException(ex, options);
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

        private static async Task HandleQuery(IOwinContext context, Guid queryId, HandlerSettings options, Func<IDictionary<string, object>, Type> getInputType,
            Func<IDictionary<string, object>, Type> getOutputType)
        {
            var inputType = getInputType(context.Environment);

            var outputType = getOutputType(context.Environment);

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
