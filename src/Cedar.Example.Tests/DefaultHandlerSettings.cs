namespace Cedar.Example.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.ContentNegotiation;
    using Cedar.Handlers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class DefaultHandlerSettings : HandlerSettings
    {
        private readonly JsonSerializer _jsonSerializer;

        internal DefaultHandlerSettings(
            [NotNull] HandlerModule handlerModule,
            [NotNull] IContentTypeMapper contentTypeMapper,
            IExceptionToModelConverter exceptionToModelConverter = null,
            JsonSerializerSettings serializerSettings = null)
            : this(Enumerable.Repeat(handlerModule, 1), contentTypeMapper, exceptionToModelConverter, serializerSettings)
        {}

        internal DefaultHandlerSettings(
            [NotNull] IEnumerable<HandlerModule> handlerModules,
            [NotNull] IContentTypeMapper contentTypeMapper,
            IExceptionToModelConverter exceptionToModelConverter = null,
            JsonSerializerSettings serializerSettings = null)
            : base(handlerModules, contentTypeMapper, exceptionToModelConverter)
        {
            _jsonSerializer = JsonSerializer.Create(serializerSettings ?? new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                TypeNameHandling = TypeNameHandling.None
            });
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