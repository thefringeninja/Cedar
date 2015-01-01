namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;

    internal class CommandHandlerRegistration
    {
        private static readonly IEqualityComparer<CommandHandlerRegistration> MessageTypeComparerInstance =
            new MessageTypeEqualityComparer();

        private readonly object _handlerInstance;
        private readonly Type _messageType;
        private readonly Type _registrationType;

        internal CommandHandlerRegistration(Type messageType, Type registrationType, object handlerInstance)
        {
            _messageType = messageType;
            _registrationType = registrationType;
            _handlerInstance = handlerInstance;
        }

        internal static IEqualityComparer<CommandHandlerRegistration> MessageTypeComparer
        {
            get { return MessageTypeComparerInstance; }
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

        private sealed class MessageTypeEqualityComparer : IEqualityComparer<CommandHandlerRegistration>
        {
            public bool Equals(CommandHandlerRegistration x, CommandHandlerRegistration y)
            {
                if(ReferenceEquals(x, y))
                {
                    return true;
                }
                if(ReferenceEquals(x, null))
                {
                    return false;
                }
                if(ReferenceEquals(y, null))
                {
                    return false;
                }
                if(x.GetType() != y.GetType())
                {
                    return false;
                }
                return x._messageType == y._messageType;
            }

            public int GetHashCode(CommandHandlerRegistration obj)
            {
                return obj._messageType.GetHashCode();
            }
        }
    }
}