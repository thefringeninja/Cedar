namespace Cedar
{
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.Serialization.Client;
    using Cedar.TypeResolution;

    public abstract class HandlerSettings
    {
        private readonly IEnumerable<IHandlerResolver> _handlerModules;
        private readonly IRequestTypeResolver _requestTypeResolver;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;
        private readonly ISerializer _serializer;

        protected HandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] IRequestTypeResolver requestTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
            ISerializer serializer = null)
        {
            Guard.EnsureNotNull(handlerModules, "handlerResolver");
            Guard.EnsureNotNull(requestTypeResolver, "requestTypeResolver");

            _handlerModules = handlerModules;
            _requestTypeResolver = requestTypeResolver;
            _exceptionToModelConverter = exceptionToModelConverter ?? new ExceptionToModelConverter();
            _serializer = serializer ?? new DefaultJsonSerializer();
        }

        public IEnumerable<IHandlerResolver> HandlerModules
        {
            get { return _handlerModules; }
        }

        public IRequestTypeResolver RequestTypeResolver
        {
            get { return _requestTypeResolver; }
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