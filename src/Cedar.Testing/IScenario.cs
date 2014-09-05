namespace Cedar.Testing
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public interface IScenario
    {
        string Name { get; }
        
        Task<ScenarioResult> Run();

        TaskAwaiter<ScenarioResult> GetAwaiter();
    }
}