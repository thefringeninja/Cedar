namespace Cedar.ProcessManagers
{
    using System.Threading;
    using System.Threading.Tasks;

    public delegate Task DispatchCommand(object command, CancellationToken cancellationToken);
}