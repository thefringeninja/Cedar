namespace Cedar.Testing.Printing
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    public interface IScenarioResultPrinter : IDisposable
    {
        Task PrintCategoryFooter(Type foundOn);
        Task PrintCategoryHeader(Type foundOn);
        Task PrintResult(ScenarioResult result);
        Task Flush();
        string FileExtension { get; }
    }
}
