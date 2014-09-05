namespace Cedar.Testing.Printing
{
    using System.Threading.Tasks;

    public interface IScenarioResultPrinter
    {
        Task PrintCategoryFooter(string category);
        Task PrintCategoryHeader(string category);
        Task PrintResult(ScenarioResult result);
    }
}
