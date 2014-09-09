namespace Cedar
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cedar.Annotations;
    using Cedar.ContentNegotiation;
    using Cedar.Handlers;
    using Newtonsoft.Json;

    internal class DefaultHandlerSettings : HandlerSettings
    {
        private readonly JsonSerializer _jsonSerializer;

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