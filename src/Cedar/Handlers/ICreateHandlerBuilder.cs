namespace Cedar.Handlers
{
    public interface ICreateHandlerBuilder
    {
        IHandlerBuilder<TMessage> For<TMessage>() where TMessage : class;
    }
}