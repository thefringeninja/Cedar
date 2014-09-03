namespace Cedar.Example.Handlers
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.Domain;
    using Cedar.Domain.Persistence;
    using Cedar.Handlers;

    public class DomainCommandHandlerModule : HandlerModule
    {
        public DomainCommandHandlerModule(Func<IRepository> repository)
        {
            For<CommandMessage<CreateAggregate>>()
                .LogExceptions()
                .DenyAnonymous()
                .RequiresClaim(claim => claim.Type == ClaimTypes.Email)
                .ValidateWith(Command1Validator.Instance)
                .Handle((message, ct) => repository().Save(new Aggregate1(message.Command), message.CommandId));

            For<CommandMessage<CancelAggregate>>()
                .LogExceptions()
                .DenyAnonymous()
                .RequiresClaim(claim => claim.Type == ClaimTypes.Email)
                .Pipe(next => next)
                .Handle((message, ct) => /* etc */ Task.FromResult(0));

            For<CommandMessage<OtherOperationOnAggregate>>()
                .PerformanceCounter()
                .LogAuthorizeAndValidate(Command1Validator.Instance)
                .RequiresClaim(claim => claim.Type == ClaimTypes.Email)
                .Handle((message, ct) => /* etc */ Task.FromResult(0));
        }
    }

    public class CreateAggregate
    {
        public static string NewItemId { get; set; }
    }

    public class Command1Validator
    {
        public static Command1Validator Instance = new Command1Validator();
    }

    public class CancelAggregate { }

    public class OtherOperationOnAggregate { }

    public class Aggregate1 : AggregateBase
    {
        protected Aggregate1(string id)
            : base(id)
        { }

        public Aggregate1(CreateAggregate command)
            : this(CreateAggregate.NewItemId)
        { }
    }
}