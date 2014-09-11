namespace Cedar
{
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.ContentNegotiation;
    using Cedar.Handlers;

    public class DefaultHandlerSettings : HandlerSettings
    {
        public DefaultHandlerSettings(
           [NotNull] IHandlerResolver handlerModule,
           [NotNull] IContentTypeMapper contentTypeMapper,
           IExceptionToModelConverter exceptionToModelConverter = null)
            : this(new[] { handlerModule }, contentTypeMapper, exceptionToModelConverter)
        {}

        public DefaultHandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] IContentTypeMapper contentTypeMapper,
            IExceptionToModelConverter exceptionToModelConverter = null)
            : base(handlerModules, contentTypeMapper, exceptionToModelConverter)
        {}

    }
}