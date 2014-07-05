namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.CommandHandling.Dispatching;
    using Cedar.CommandHandling.Modules;
    using Cedar.CommandHandling.Serialization;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.TinyIoc;

    internal class CommandHandlingNancyBootstrapper : DefaultNancyBootstrapper
    {
        private readonly string _vendorName;
        private readonly IEnumerable<Type> _commandTypes;
        private readonly ICommandHandlerResolver _handlerResolver;

        public CommandHandlingNancyBootstrapper(
            [NotNull] string vendorName,
            [NotNull] IEnumerable<Type> commandTypes,
            [NotNull] ICommandHandlerResolver handlerResolver)
        {
            _vendorName = vendorName;
            _commandTypes = commandTypes;
            _handlerResolver = handlerResolver;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register<ICommandTypeFromHttpContentType>(
                new DefaultCommandTypeFromContentTypeResolver(_vendorName, _commandTypes));
            container.Register<ICommandDispatcher, CommandDispatcher>();
            container.Register(_handlerResolver);
            container.RegisterMultiple<ICommandDeserializer>(
                new [] {typeof(JsonCommandDeserializer), typeof(XmlCommandDeserializer)});
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