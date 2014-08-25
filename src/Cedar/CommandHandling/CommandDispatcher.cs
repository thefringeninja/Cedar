namespace Cedar.CommandHandling
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Cedar.Annotations;

    public class CommandDispatcher
    {
        private readonly ICommandHandlerResolver _handlerResolver;
        private readonly MethodInfo _dispatchMethodInfo;

        public CommandDispatcher(ICommandHandlerResolver handlerResolver)
        {
            _handlerResolver = handlerResolver;
            _dispatchMethodInfo = GetType()
                .GetMethod("DispatchGeneric", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public async Task<bool> Dispatch(ICommandContext commandContext, object command)
        {
            return await (Task<bool>)_dispatchMethodInfo
                .MakeGenericMethod(command.GetType())
                .Invoke(this, new[] { commandContext, command });
        }

        [UsedImplicitly]
        private async Task<bool> DispatchGeneric<T>(ICommandContext commandContext, T command)
            where T : class
        {
            ICommandHandler<T> commandHandler = _handlerResolver.Resolve<T>();
            if (commandHandler == null)
            {
                return false;
            }
            await commandHandler.Handle(commandContext, command);
            return true;
        }
    }
}