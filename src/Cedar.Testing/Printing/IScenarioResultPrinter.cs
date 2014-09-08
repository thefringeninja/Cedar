namespace Cedar.Testing.Printing
{
    using System;
    using System.Threading.Tasks;

    public interface IScenarioResultPrinter : IDisposable
    {
        Task PrintCategoryFooter(string category);
        Task PrintCategoryHeader(string category);
        Task PrintResult(ScenarioResult result);
        Task Flush();
        string FileExtension { get; }
    }
}
