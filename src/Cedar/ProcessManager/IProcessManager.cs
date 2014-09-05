namespace Cedar.ProcessManager
{
    using System;
    using System.Threading.Tasks;

    public interface IProcessManager : IDisposable
    {
        Task RunProcess();
    }
}