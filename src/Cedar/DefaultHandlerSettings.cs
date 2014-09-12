namespace Cedar
{
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.TypeResolution;

    public class DefaultHandlerSettings : HandlerSettings
    {
        public DefaultHandlerSettings(
           [NotNull] IHandlerResolver handlerModule,
           [NotNull] IRequestTypeResolver requestTypeResolver,
           IExceptionToModelConverter exceptionToModelConverter = null)
            : this(new[] { handlerModule }, requestTypeResolver, exceptionToModelConverter)
        {}

        public DefaultHandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] IRequestTypeResolver requestTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null)
            : base(handlerModules, requestTypeResolver, exceptionToModelConverter)
        {}

    }
}