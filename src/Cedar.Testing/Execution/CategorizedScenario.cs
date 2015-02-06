namespace Cedar.Testing.Execution
{
    using System;
    using System.Threading.Tasks;

    public class CategorizedScenario
    {
        public readonly Type Category;
        public readonly Task<ScenarioResult> Run;

        public CategorizedScenario(Type category, Task<ScenarioResult> run)
        {
            Category = category;
            Run = run;
        }
    }
}