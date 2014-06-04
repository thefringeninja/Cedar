namespace Cedar
{
    using System;
    using Cedar.Hosting;

    public class CedarEngine : IDisposable
    {
        private readonly CedarBootstrapper _bootstrapper;

        public CedarEngine(CedarBootstrapper bootstrapper)
        {
            Guard.EnsureNotNull(bootstrapper, "bootstrapper");

            _bootstrapper = bootstrapper;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}