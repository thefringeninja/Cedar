namespace Cedar.Testing
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IScenario
    {
        string Name { get; }
        Task Print(TextWriter writer);
        Task Run();
    }
}