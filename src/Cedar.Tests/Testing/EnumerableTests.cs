namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class EnumerableTests
    {
        public IEnumerable<Task<ScenarioResult>> test_method()
        {
            return Enumerable.Range(0, 5)
                .Select(async _ => await Scenario.For<DateTime>()
                    .Given(() => new DateTime(2000, 1, 1))
                    .When(date => date.AddDays(1))
                    .ThenShouldEqual(new DateTime(2000, 1, 1)));
        }
    }
}