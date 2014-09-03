namespace Cedar.Handlers
{
    public interface IHandlerBuilder<TMessage>
    {
        IHandlerBuilder<TMessage> Pipe(Pipe<TMessage> pipe);

        void Handle(Handler<TMessage> handler);
    }
}