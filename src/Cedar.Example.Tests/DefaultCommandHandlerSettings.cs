namespace Cedar.Example.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Handlers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class DefaultCommandHandlerSettings : CommandHandlerSettings
    {
        private readonly JsonSerializer _jsonSerializer;

        internal DefaultCommandHandlerSettings(
            [NotNull] HandlerModule handlerModule,
            [NotNull] ICommandTypeResolver commandTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
            JsonSerializerSettings serializerSettings = null)
            : this(Enumerable.Repeat(handlerModule, 1), commandTypeResolver, exceptionToModelConverter, serializerSettings)
        {}

        internal DefaultCommandHandlerSettings(
            [NotNull] IEnumerable<HandlerModule> handlerModules,
            [NotNull] ICommandTypeResolver commandTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
            JsonSerializerSettings serializerSettings = null)
            : base(handlerModules, commandTypeResolver, exceptionToModelConverter)
        {
            _jsonSerializer = JsonSerializer.Create(serializerSettings ?? new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                TypeNameHandling = TypeNameHandling.All
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