namespace Cedar.Handlers
{
    using System;

    internal class HandlerRegistration
    {
        private readonly object _handlerInstance;
        private readonly Type _messageType;
        private readonly Type _registrationType;

        internal HandlerRegistration(Type messageType, Type registrationType, object handlerInstance)
        {
            _messageType = messageType;
            _registrationType = registrationType;
            _handlerInstance = handlerInstance;
        }

        public Type RegistrationType
        {
            get { return _registrationType; }
        }

        public Type MessageType
        {
            get { return _messageType; }
        }

        public object HandlerInstance
        {
            get { return _handlerInstance; }
        }
    }
}