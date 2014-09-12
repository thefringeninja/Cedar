namespace Cedar.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Cedar.TypeResolution;
    using Microsoft.Owin;

    public static class QueryTypeMapping
    {
        public static Func<IDictionary<string, object>, Type> InputTypeFromPathSegment(HandlerSettings options, string queryPath)
        {
            return env =>
            {
                var context = new OwinContext(env);
                
                var type = options.RequestTypeResolver.ResolveInputType(new CedarRequest(context));
                if (type == null)
                {
                    // because we are using the path here and the type is not registered, the resource "doesn't exist"
                    throw new HttpStatusException("The requested resource was not found.", HttpStatusCode.NotFound, new NotSupportedException());
                }

                if (!context.Request.ContentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase))
                {
                    // Not a json entity
                    throw new HttpStatusException("The specified media type is not supported.", HttpStatusCode.UnsupportedMediaType, new NotSupportedException());
                }

                return type;
            };
        }

        public static Func<IDictionary<string, object>, Type> OutputTypeFromAcceptHeader(HandlerSettings options)
        {
            return env =>
            {
                var context = new OwinContext(env);
                var type = options.RequestTypeResolver.ResolveOutputType(new CedarRequest(context));

                if (type == null)
                {
                    throw new HttpStatusException("The requested media type is not acceptable.", HttpStatusCode.UnsupportedMediaType, new NotSupportedException());
                }

                return type;
            };
        }
    }
}