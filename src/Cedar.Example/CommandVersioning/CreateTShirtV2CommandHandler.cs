namespace Cedar.Example.CommandVersioning
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.Handlers;

    public class CreateTShirtV2CommandHandler : IHandler<CommandMessage<CreateTShirtV2>>
    {
        public Task Handle(CommandMessage<CreateTShirtV2> commandMessage, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}