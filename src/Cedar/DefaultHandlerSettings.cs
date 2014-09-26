namespace Cedar
{
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.Serialization.Client;
    using Cedar.TypeResolution;

    public class DefaultHandlerSettings : HandlerSettings
    {
        public DefaultHandlerSettings(
           [NotNull] IHandlerResolver handlerModule,
           [NotNull] IRequestTypeResolver requestTypeResolver,
           IExceptionToModelConverter exceptionToModelConverter = null,
           ISerializer serializer = null)
            : this(new[] { handlerModule }, requestTypeResolver, exceptionToModelConverter, serializer)
        {}

        public DefaultHandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] IRequestTypeResolver requestTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
           ISerializer serializer = null)
            : base(handlerModules, requestTypeResolver, exceptionToModelConverter, serializer)
        {}

    }
}