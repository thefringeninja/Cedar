namespace Cedar.Hosting
{
    using Cedar.CommandHandling;
    using Nancy.TinyIoc;

    public static class TinyIoCExtensions
    {
        public static void RegisterCommandHander<TCommand, TCommandHandler>(this TinyIoCContainer container)
            where TCommand : class
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            container.RegisterMultiple<ICommandHandler<TCommand>>(new[]{typeof(TCommandHandler)});
            container.Register<TCommandHandler>();  
        }
    }
}
