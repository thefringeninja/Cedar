namespace Cedar.Example
{
    public interface IEventPublisher
    {
        void Publish(object @event);
    }
}