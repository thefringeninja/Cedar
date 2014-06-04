namespace Cedar.CommandHandling.Dispatching
{
    using System;
    using System.Security.Claims;
    using System.Threading;

    public interface ICommandContext
    {
        Guid CommandId { get; }

        CancellationToken CancellationToken { get; }

        ClaimsPrincipal User { get; }
    }
}