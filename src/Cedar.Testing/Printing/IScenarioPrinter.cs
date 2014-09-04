namespace Cedar.Testing.Printing
{
    using System;
    using System.Threading.Tasks;

    public interface IScenarioPrinter
    {
        Task WriteHeader();
        Task WriteFooter();
        Task WriteScenarioName(string name, bool passed);
        Task WriteGiven(object given);
        Task WriteWhen(object when);
        Task WriteExpect(object expect);
        Task WriteOcurredException(Exception occurredException);
        Task WriteStartCategory(string category);
        Task WriteEndCategory(string category);
    }
}
