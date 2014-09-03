namespace Cedar.Handlers
{
    public interface IHandlerBuilder<TMessage>
    {
        IHandlerBuilder<TMessage> Pipe(Pipe<TMessage> pipe);

        ICreateHandlerBuilder Handle(Handler<TMessage> handler);
    }
}