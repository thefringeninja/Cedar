namespace Cedar.Testing.Printing
{
    using System;
    using System.Threading.Tasks;

    public interface IScenarioPrinter
    {
        Task WriteFooter();
        Task WriteHeader(string scenarioName, TimeSpan? duration, bool passed);
        Task WriteGiven(object given);
        Task WriteWhen(object when);
        Task WriteExpect(object expect);
        Task WriteOcurredException(Exception occurredException);
        Task WriteStartCategory(string category);
        Task WriteEndCategory(string category);
    }
}
