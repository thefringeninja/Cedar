namespace Cedar.Example.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Annotations;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.TypeResolution;

    internal class DefaultHandlerConfiguration : HandlerConfiguration
    {
        internal DefaultHandlerConfiguration(
            [NotNull] IHandlerResolver handlerModule,
            [NotNull] IRequestTypeResolver requestTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null)
            : this(Enumerable.Repeat(handlerModule, 1), requestTypeResolver, exceptionToModelConverter)
        {}

        internal DefaultHandlerConfiguration(
            [NotNull] IEnumerable<IHandlerResolver> handlerResolvers,
            [NotNull] IRequestTypeResolver requestTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null)
            : base(handlerResolvers, requestTypeResolver, exceptionToModelConverter)
        {
        }
    }
}