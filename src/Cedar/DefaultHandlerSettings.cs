namespace Cedar
{
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.ContentNegotiation;
    using Cedar.Handlers;

    internal class DefaultHandlerSettings : HandlerSettings
    {
        internal DefaultHandlerSettings(
           [NotNull] IHandlerResolver handlerModule,
           [NotNull] IContentTypeMapper contentTypeMapper,
           IExceptionToModelConverter exceptionToModelConverter = null)
            : this(new[] { handlerModule }, contentTypeMapper, exceptionToModelConverter)
        {}

        internal DefaultHandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] IContentTypeMapper contentTypeMapper,
            IExceptionToModelConverter exceptionToModelConverter = null)
            : base(handlerModules, contentTypeMapper, exceptionToModelConverter)
        {
        }

    }
}