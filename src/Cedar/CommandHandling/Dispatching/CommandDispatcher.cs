namespace Cedar.CommandHandling.Dispatching
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using Cedar.Extensions;
    using Cedar.Hosting;
    using Cedar.Properties;

    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly ICommandHandlerResolver _handlerResolver;
        private readonly MethodInfo _dispatchMethodInfo;

        public CommandDispatcher(ICommandHandlerResolver handlerResolver)
        {
            _handlerResolver = handlerResolver;
            _dispatchMethodInfo = GetType()
                .GetMethod("DispatchGeneric", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public async Task Dispatch(ICommandContext commandContext, object command)
        {
            try
            {
                await (Task)_dispatchMethodInfo
                    .MakeGenericMethod(command.GetType())
                    .Invoke(this, new[] {commandContext, command});
            }
            catch(TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }

        [UsedImplicitly]
        private Task DispatchGeneric<T>(ICommandContext commandContext, T command)
            where T : class
        {
            ICommandHandler<T> commandHandler = _handlerResolver.Resolve<T>();
            if(commandHandler == null)
            {
                throw new InvalidOperationException(Messages.CommandHandlerNotFound.FormatWith(typeof(T).FullName));
            }
            return commandHandler.Handle(commandContext, command);
        }
    }
}
