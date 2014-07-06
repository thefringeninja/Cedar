namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.Client;
    using Cedar.CommandHandling.Dispatching;
    using Cedar.CommandHandling.Modules;
    using Cedar.CommandHandling.Serialization;
    using Cedar.Exceptions;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.Responses;
    using Nancy.Serialization.JsonNet;
    using Nancy.TinyIoc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class CommandHandlingNancyBootstrapper : DefaultNancyBootstrapper
    {
        private readonly string _vendorName;
        private readonly IEnumerable<Type> _commandTypes;
        private readonly ICommandHandlerResolver _handlerResolver;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;

        public CommandHandlingNancyBootstrapper(
            [NotNull] string vendorName,
            [NotNull] IEnumerable<Type> commandTypes,
            [NotNull] ICommandHandlerResolver handlerResolver,
            [NotNull] IExceptionToModelConverter exceptionToModelConverter)
        {
            _vendorName = vendorName;
            _commandTypes = commandTypes;
            _handlerResolver = handlerResolver;
            _exceptionToModelConverter = exceptionToModelConverter;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register(JsonSerializer.Create(DefaultJsonSerializerSettings.Settings));
            container.Register<ICommandTypeFromHttpContentType>(
                new DefaultCommandTypeFromContentTypeResolver(_vendorName, _commandTypes));
            container.Register<ICommandDispatcher, CommandDispatcher>();
            container.Register(_handlerResolver);
            container.Register(_exceptionToModelConverter);
            container.RegisterMultiple<ICommandDeserializer>(
                new [] {typeof(JsonCommandDeserializer), typeof(XmlCommandDeserializer)});
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(c =>
                {
                    c.Serializers = new[]
                    {
                        typeof(DefaultXmlSerializer),
                        typeof(JsonNetSerializer)
                    };
                });
            }
        }

        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                return new[] {new ModuleRegistration(typeof(CommandModule))};
            }
        }
    }
}