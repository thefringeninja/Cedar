namespace Cedar.Testing
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class ValueObjectTests
    {
        [Fact]
        public async Task a_passing_value_object_scenario_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddDays(1))
                .ThenShouldEqual(new DateTime(2000, 1, 2));

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_passing_value_object_with_transformation_scenario_should()
        {
            var result = await Scenario.For<DateTime, long>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddDays(1).Ticks)
                .ThenShouldEqual(new DateTime(2000, 1, 2).Ticks);

            Assert.True(result.Passed);
        }


        [Fact]
        public async Task a_failing_value_object_scenario_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddDays(1))
                .ThenShouldEqual(new DateTime(2000, 1, 3));

            Assert.False(result.Passed);
        }

        [Fact]
        public async Task a_value_object_throwing_an_exception_in_when_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddYears(Int32.MinValue))
                .ThenShouldEqual(new DateTime(2000, 1, 3));

            Assert.False(result.Passed);
            Assert.IsType<ScenarioException>(result.Results);
        }

        [Fact]
        public async Task a_value_object_throwing_an_exception_in_given_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(Int32.MinValue, 1, 1))
                .When(date => date.AddYears(Int32.MinValue))
                .ThenShouldThrow<ArgumentOutOfRangeException>();

            Assert.False(result.Passed);
            Assert.IsType<ScenarioException>(result.Results);
        }

        [Fact]
        public async Task a_value_object_throwing_an_expected_exception_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddYears(Int32.MinValue))
                .ThenShouldThrow<ArgumentOutOfRangeException>();

            Assert.True(result.Passed);
            Assert.IsType<ArgumentOutOfRangeException>(result.Results);
        }

        [Fact]
        public async Task a_value_object_with_no_when_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .ThenShouldEqual(new DateTime(2000, 1, 1));

            Assert.True(result.Passed);
        }

    }
}