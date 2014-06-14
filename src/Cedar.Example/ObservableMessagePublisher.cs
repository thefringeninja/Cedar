namespace Cedar.Example
{
    using System;

    public class ObservableMessagePublisher : IEventPublisher
    {
        private readonly IObserver<object> _messages;

        public ObservableMessagePublisher(IObserver<object> messages)
        {
            _messages = messages;
        }

        #region IEventPublisher Members

        public void Publish(object @event)
        {
            _messages.OnNext(@event);
        }

        #endregion
    }
}
