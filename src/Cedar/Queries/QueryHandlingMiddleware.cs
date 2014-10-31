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
            typeof(QueryHandlingMiddleware).GetMethod("WidenTaskResult", BindingFlags.Static | BindingFlags.NonPublic);

        public static MidFunc HandleQueries(HandlerSettings options,
            Func<IDictionary<string, object>, Type> getInputType = null,
            Func<IDictionary<string, object>, Type> getOutputType = null,
            Func<IRequest, Stream> getInputStream = null,
            string queryPath = "/query")
        {
            Guard.EnsureNotNull(options, "options");

            var acceptableMethods = new[] { "GET", "POST" };

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

                return BuildHandlerCall(getInputStream)
                    .ExecuteWithExceptionHandling(context, options);
            };
        }

        private static Func<IOwinContext, HandlerSettings, Task> BuildHandlerCall(Func<IRequest, Stream> getInputStream = null)
        {
            return (context, options) => HandleQuery(
                context,
                Guid.NewGuid(),
                options,
                getInputStream);
        }

        private static async Task HandleQuery(IOwinContext context, Guid queryId, HandlerSettings options, Func<IRequest, Stream> getInputStream = null)
        {
            getInputStream = getInputStream ?? DefaultGetInputStream;

            var request = new CedarRequest(context);

            var inputType = options.RequestTypeResolver.ResolveInputType(request);

            if (inputType == null)
            {
                throw new HttpStatusException("Unable to find the query type", HttpStatusCode.BadRequest, new NotSupportedException());
            }

            var outputType = options.RequestTypeResolver.ResolveOutputType(request);

            if(outputType == null)
            {
                throw new HttpStatusException(string.Format("Unable to find type {0}Response.", inputType.FullName), HttpStatusCode.NotAcceptable, new NotSupportedException());
            }

            object input;

            using (var streamReader = new StreamReader(getInputStream(request)))
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

            var result = await (Task<object>)WidenTaskResultMethodInfo
                .MakeGenericMethod(outputType)
                .Invoke(null, new[] { task });

            if(result == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                context.Response.ReasonPhrase = "Not Found";
            }
            else
            {
                var body = options.Serializer.Serialize(result);

                context.Response.Headers["ETag"] = string.Format("\"{0}\"", body.GetHashCode());

                if (Fresh(context.Request, context.Response))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                    context.Response.ReasonPhrase = "NotModified";
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ReasonPhrase = "OK";
                    await context.Response.WriteAsync(body);
                }
            }
        }

        private static bool Fresh(IOwinRequest request, IOwinResponse response)
        {
            if ((response.StatusCode < 200 || response.StatusCode >= 300) && response.StatusCode != 304)
            {
                return false;
            }

            var requestEtags = IfNoneMatch(request);
            var responseEtag = response.Headers["ETag"];

            return requestEtags.Contains(responseEtag);
        }

        private static IEnumerable<string> IfNoneMatch(IOwinRequest owinRequest)
        {
            var ifNoneMatch = owinRequest.Headers["If-None-Match"];

            if (ifNoneMatch != null)
            {
                return ifNoneMatch.Split(',');
            }

            return Enumerable.Empty<string>();
        }

        private static Stream DefaultGetInputStream(IRequest request)
        {
            return request.Body;
        }

        [UsedImplicitly]
        private static async Task<object> WidenTaskResult<TResult>(Task<TResult> task)
        {
            return await task;
        }

    }
}
