namespace Cedar.Example.CommandVersioning
{
    using System.Threading.Tasks;
    using Cedar.CommandHandling;
    using Cedar.CommandHandling.Dispatching;
    
    public class CreateTShirtV3CommandHandler : ICommandHandler<CreateTShirtV3>
    {
        private readonly CreateTShirtV4CommandHandler next;

        public CreateTShirtV3CommandHandler(CreateTShirtV4CommandHandler next)
        {
            this.next = next;
        }

        #region ICommandHandler<CreateTShirtV3> Members

        public Task Handle(ICommandContext context, CreateTShirtV3 command)
        {
            return next.Handle(
                context,
                new CreateTShirtV4
                {
                    Name = command.Name,
                    Sizes = command.Sizes,
                    Colors = command.Colors,
                    BlankType = "Round"
                });
        }

        #endregion
    }
}