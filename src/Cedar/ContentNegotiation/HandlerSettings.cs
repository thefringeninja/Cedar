namespace Cedar.ContentNegotiation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Handlers;

    public abstract class HandlerSettings
    {
        private readonly IEnumerable<IHandlerResolver> _handlerModules;
        private readonly IContentTypeMapper _contentTypeMapper;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;

        protected HandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] IContentTypeMapper contentTypeMapper,
            IExceptionToModelConverter exceptionToModelConverter)
        {
            Guard.EnsureNotNull(handlerModules, "handlerResolver");
            Guard.EnsureNotNull(contentTypeMapper, "contentTypeMapper");

            _handlerModules = handlerModules;
            _contentTypeMapper = contentTypeMapper;
            _exceptionToModelConverter = exceptionToModelConverter ?? new ExceptionToModelConverter();
        }

        public IEnumerable<IHandlerResolver> HandlerModules
        {
            get { return _handlerModules; }
        }

        public IContentTypeMapper ContentTypeMapper
        {
            get { return _contentTypeMapper; }
        }

        public IExceptionToModelConverter ExceptionToModelConverter
        {
            get { return _exceptionToModelConverter; }
        }

        public abstract object Deserialize(TextReader reader, Type type);

        public abstract void Serialize(TextWriter writer, object target);
    }
}