namespace Cedar
{
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.ContentNegotiation;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.Serialization.Client;

    public abstract class HandlerSettings
    {
        private readonly IEnumerable<IHandlerResolver> _handlerModules;
        private readonly IContentTypeMapper _contentTypeMapper;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;
        private readonly ISerializer _serializer;

        protected HandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] IContentTypeMapper contentTypeMapper,
            IExceptionToModelConverter exceptionToModelConverter = null,
            ISerializer serializer = null)
        {
            Guard.EnsureNotNull(handlerModules, "handlerResolver");
            Guard.EnsureNotNull(contentTypeMapper, "contentTypeMapper");

            _handlerModules = handlerModules;
            _contentTypeMapper = contentTypeMapper;
            _exceptionToModelConverter = exceptionToModelConverter ?? new ExceptionToModelConverter();
            _serializer = serializer ?? new DefaultJsonSerializer();
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

        public ISerializer Serializer
        {
            get { return _serializer; }
        }
    }
}