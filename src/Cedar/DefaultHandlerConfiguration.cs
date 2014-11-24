namespace Cedar
{
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.Serialization;
    using Cedar.TypeResolution;

    public class DefaultHandlerConfiguration : HandlerConfiguration
    {
        public DefaultHandlerConfiguration(
           [NotNull] IHandlerResolver handlerModule,
           [NotNull] IRequestTypeResolver requestTypeResolver,
           IExceptionToModelConverter exceptionToModelConverter = null,
           ISerializer serializer = null)
            : this(new[] { handlerModule }, requestTypeResolver, exceptionToModelConverter, serializer)
        {}

        public DefaultHandlerConfiguration(
            [NotNull] IEnumerable<IHandlerResolver> handlerResolvers,
            [NotNull] IRequestTypeResolver requestTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
           ISerializer serializer = null)
            : base(handlerResolvers, requestTypeResolver, exceptionToModelConverter, serializer)
        {}

    }
}