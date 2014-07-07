namespace Cedar.Example.CommandVersioning
{
    using TinyIoC;

    public class Bootstrapper : CedarBootstrapper
    {
        private readonly IEventPublisher _events;

        public Bootstrapper(IEventPublisher events)
        {
            _events = events;
        }

        public override string VendorName
        {
            get { return "cedar"; }
        }

        public override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register(_events);
        }
    }
}
