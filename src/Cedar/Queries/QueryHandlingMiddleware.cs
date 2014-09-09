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

        public static MidFunc HandleCommands(HandlerSettings options, string queryPath = "/query")
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
                    return HandleQuery(Guid.NewGuid(), options, context);
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

        private static async Task HandleQuery(Guid queryId, HandlerSettings options, IOwinContext context)
        {
            string contentType = context.Request.ContentType;
            var inputType = options.ContentTypeMapper.GetFromContentType(contentType);
            if (!contentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase) || inputType == null)
            {
                // Not a json entity OR not content type not registered, unsupported
                await context.HandleUnsupportedMediaType(new NotSupportedException(), options);
                return;
            }
            
            string accepts = context.Request.Accept;

            // TODO: JPB parse accept header correctly
            var outputType = options.ContentTypeMapper.GetFromContentType(accepts);
            if (outputType == null)
            {
                await context.HandleNotAcceptable(new NotSupportedException(), options);
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
            context.Response.ContentType = accepts;
        }
    }
}
