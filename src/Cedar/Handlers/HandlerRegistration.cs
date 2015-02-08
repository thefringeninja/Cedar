namespace Cedar.Handlers
{
    using System;

    internal class HandlerRegistration
    {
        private readonly object _handlerInstance;
        private readonly Type _registrationType;

        internal HandlerRegistration(Type registrationType, object handlerInstance)
        {
            _registrationType = registrationType;
            _handlerInstance = handlerInstance;
        }

        public Type RegistrationType
        {
            get { return _registrationType; }
        }

        public object HandlerInstance
        {
            get { return _handlerInstance; }
        }
    }
}