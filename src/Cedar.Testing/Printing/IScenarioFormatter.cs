namespace Cedar.Testing.Printing
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IScenarioFormatter
    {
        Task WriteHeader(TextWriter writer);
        Task WriteFooter(TextWriter writer);
        Task WriteScenarioName(string name, TextWriter writer);
        Task WriteGiven(object given, TextWriter writer);
        Task WriteWhen(object when, TextWriter writer);
        Task WriteExpect(object expect, TextWriter writer);
        Task WriteOcurredException(Exception occurredException, TextWriter writer);
    }
}
