namespace Cedar.Example.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Annotations;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.TypeResolution;

    internal class DefaultHandlerSettings : HandlerSettings
    {
        internal DefaultHandlerSettings(
            [NotNull] IHandlerResolver handlerModule,
            [NotNull] IRequestTypeResolver requestTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null)
            : this(Enumerable.Repeat(handlerModule, 1), requestTypeResolver, exceptionToModelConverter)
        {}

        internal DefaultHandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] IRequestTypeResolver requestTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null)
            : base(handlerModules, requestTypeResolver, exceptionToModelConverter)
        {
        }
    }
}