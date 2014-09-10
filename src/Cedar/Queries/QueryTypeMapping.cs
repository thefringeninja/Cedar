namespace Cedar.Queries
{
    using System;
    using System.Threading.Tasks;
    using Cedar.ContentNegotiation;
    using Microsoft.Owin;

    public static class QueryTypeMapping
    {
        public static Func<IOwinContext, Task<Type>> InputTypeFromPathSegment(HandlerSettings options, string queryPath)
        {
            return async context =>
            {
                if (!context.Request.ContentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase))
                {
                    // Not a json entity
                    await context.HandleUnsupportedMediaType(new NotSupportedException(), options);
                    return null;
                }
                
                var type = options.ContentTypeMapper.GetFromContentType(context.Request.Path.Value.Remove(0, queryPath.Length + 1));

                if (type == null)
                {
                    // because we are using the path here and the type is not registered, the resource "doesn't exist"
                    await context.HandleNotFound(new NotSupportedException(), options);
                }

                return type;
            };
        }

        public static Func<IOwinContext, Task<Type>> OutputTypeFromAcceptHeader(HandlerSettings options)
        {
            return async context =>
            {
                ;
                var type = options.ContentTypeMapper.FindBest(context);

                if (type == null)
                {
                    await context.HandleNotAcceptable(new NotSupportedException(), options);
                }

                return type;
            };
        }
    }
}