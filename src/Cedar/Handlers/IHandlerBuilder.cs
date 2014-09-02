namespace Cedar.Handlers
{
    public interface IHandlerBuilder<TMessage>
    {
        IHandlerBuilder<TMessage> Handle(HandlerMiddleware<TMessage> middleware);

        void Finally(Handler<TMessage> finalHandler);
    }
}