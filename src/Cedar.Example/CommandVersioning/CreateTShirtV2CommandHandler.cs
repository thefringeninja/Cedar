namespace Cedar.Example.CommandVersioning
{
    using System;
    using System.Threading.Tasks;
    using Cedar.CommandHandling;

    public class CreateTShirtV2CommandHandler : ICommandHandler<CreateTShirtV2>
    {
        public Task Handle(ICommandContext context, CreateTShirtV2 command)
        {
            throw new NotSupportedException();
        }
    }
}