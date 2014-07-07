namespace Cedar.Hosting
{
    using Cedar.CommandHandling;
    using TinyIoC;

    public static class TinyIoCExtensions
    {
        public static void RegisterCommandHandler<TCommand, TCommandHandler>(this TinyIoCContainer container)
            where TCommand : class
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            container.RegisterMultiple<ICommandHandler<TCommand>>(new[]{typeof(TCommandHandler)});
            container.Register<TCommandHandler>();  
        }
    }
}
