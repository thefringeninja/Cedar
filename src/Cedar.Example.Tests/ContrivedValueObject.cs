namespace Cedar.Example.Tests
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Testing;
    using Xunit;

    public class ContrivedValueObject
    {
        [Fact]
        public async Task Passes()
        {
            await Scenario.For<DateTime>()
                .Given(new DateTime(2000, 1, 1))
                .When(date => Task.FromResult(date.AddDays(1)))
                .ThenShouldEqual(new DateTime(2000, 1, 2));
        }
        [Fact]
        public async Task<ScenarioResult> AlsoPasses()
        {
            return await Scenario.For<DateTime>()
                .Given(new DateTime(2000, 1, 1))
                .When(date => Task.FromResult(date.AddDays(1)))
                .ThenShouldEqual(new DateTime(2000, 1, 2));
        }

        [Fact]
        public async Task DoesNotPass()
        {
            await Scenario.For<DateTime>()
                .Given(new DateTime(2000, 1, 1))
                .When(date => Task.FromResult(date.AddDays(1)))
                .ThenShouldEqual(new DateTime(2000, 1, 3));
        }
        [Fact]
        public async Task<ScenarioResult> AlsoDoesNotPass()
        {
            return await Scenario.For<DateTime>()
                .Given(new DateTime(2000, 1, 1))
                .When(date => Task.FromResult(date.AddDays(1)))
                .ThenShouldEqual(new DateTime(2000, 1, 3));
        }
    }
}