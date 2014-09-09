namespace Cedar.ContentNegotiation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cedar.Annotations;
    using Cedar.Handlers;
    using Newtonsoft.Json;

    internal class DefaultHandlerSettings : HandlerSettings
    {
        private readonly JsonSerializer _jsonSerializer;

        internal DefaultHandlerSettings(
           [NotNull] IHandlerResolver handlerModule,
           [NotNull] IContentTypeMapper contentTypeMapper,
           IExceptionToModelConverter exceptionToModelConverter = null,
           JsonSerializerSettings serializerSettings = null)
            : this(new[] { handlerModule }, contentTypeMapper, exceptionToModelConverter, serializerSettings)
        {}

        internal DefaultHandlerSettings(
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] IContentTypeMapper contentTypeMapper,
            IExceptionToModelConverter exceptionToModelConverter = null,
            JsonSerializerSettings serializerSettings = null)
            : base(handlerModules, contentTypeMapper, exceptionToModelConverter)
        {
            _jsonSerializer = JsonSerializer.Create(serializerSettings ?? DefaultJsonSerializerSettings.Settings);
        }

        public override object Deserialize(TextReader reader, Type type)
        {
            return _jsonSerializer.Deserialize(reader, type);
        }

        public override void Serialize(TextWriter writer, object target)
        {
            _jsonSerializer.Serialize(writer, target);
        }
    }
}