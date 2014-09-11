namespace Cedar.Example.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Annotations;
    using Cedar.ContentNegotiation;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;

    internal class DefaultHandlerSettings : HandlerSettings
    {
        internal DefaultHandlerSettings(
            [NotNull] IHandlerResolver handlerModule,
            [NotNull] IContentTypeMapper contentTypeMapper,
            IExceptionToModelConverter exceptionToModelConverter = null)
            : this(Enumerable.Repeat(handlerModule, 1), contentTypeMapper, exceptionToModelConverter)
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