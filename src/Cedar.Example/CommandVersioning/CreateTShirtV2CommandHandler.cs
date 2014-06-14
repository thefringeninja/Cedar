namespace Cedar.Example.CommandVersioning
{
    using System;
    using System.Threading.Tasks;
    using Cedar.CommandHandling;
    using Cedar.CommandHandling.Dispatching;

    public class CreateTShirtV2CommandHandler : ICommandHandler<CreateTShirtV2>
    {
        #region ICommandHandler<CreateTShirtV2> Members

        public Task Handle(ICommandContext context, CreateTShirtV2 command)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}