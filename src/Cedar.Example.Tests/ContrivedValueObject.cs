namespace Cedar.Example.Tests
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Testing;
    using Xunit;

    public class ContrivedValueObject
    {
        [Fact]
        public async Task Works()
        {
            await Scenario.For<DateTime>()
                .Given(new DateTime(2000, 1, 1))
                .When(date => Task.FromResult(date.AddDays(1)))
                .ThenShouldEqual(new DateTime(2000, 1, 3));
        }
    }
}