namespace Cedar.Commands
{
    using System;
    using System.IO;
    using Cedar.Annotations;
    using Cedar.Commands.Client;
    using Cedar.Handlers;
    using Newtonsoft.Json;

    internal class DefaultCommandHandlerSettings : CommandHandlerSettings
    {
        private readonly JsonSerializer _jsonSerializer;

        internal DefaultCommandHandlerSettings(
            [NotNull] IHandlerResolver handlerResolver,
            [NotNull] ICommandTypeResolver commandTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
            JsonSerializerSettings serializerSettings = null)
            : base(handlerResolver, commandTypeResolver, exceptionToModelConverter)
        {
            JsonSerializerSettings serializerSettings1 = serializerSettings ?? DefaultJsonSerializerSettings.Settings;

            _jsonSerializer = JsonSerializer.Create(serializerSettings1);
        }

        public override object Deserialize(TextReader reader, Type type)
        {
            return _jsonSerializer.Deserialize(reader, type);
        }
    }
}