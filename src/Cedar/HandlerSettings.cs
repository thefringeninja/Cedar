namespace Cedar
{
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.Serialization;
    using Cedar.TypeResolution;

    public class HandlerSettings
    {
        private readonly IEnumerable<IHandlerResolver> _handlerResolvers;
        private readonly IRequestTypeResolver _requestTypeResolver;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;
        private readonly ISerializer _serializer;

        public HandlerSettings(
           [NotNull] IHandlerResolver handlerModule,
           [NotNull] IRequestTypeResolver requestTypeResolver,
           IExceptionToModelConverter exceptionToModelConverter = null,
           ISerializer serializer = null)
            : this(new[] { handlerModule }, requestTypeResolver, exceptionToModelConverter, serializer)
        {}

        public HandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerResolvers,
            [NotNull] IRequestTypeResolver requestTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
            ISerializer serializer = null)
        {
            Guard.EnsureNotNull(handlerResolvers, "handlerResolver");
            Guard.EnsureNotNull(requestTypeResolver, "requestTypeResolver");

            _handlerResolvers = handlerResolvers;
            _requestTypeResolver = requestTypeResolver;
            _exceptionToModelConverter = exceptionToModelConverter ?? new ExceptionToModelConverter();
            _serializer = serializer ?? new DefaultJsonSerializer();
        }

        public IEnumerable<IHandlerResolver> HandlerResolvers
        {
            get { return _handlerResolvers; }
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