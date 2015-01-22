namespace Cedar.Domain
{
    using FluentAssertions;
    using Xunit;

    public class AggregateTests
    {
        [Fact]
        public void should_not_require_an_explicit_handler()
        {
            var aggregate = new Aggregate("id");

            aggregate.Command();

            aggregate.Version.Should().Be(1);
        }

        private class EventThatDoesNotHaveAnApply
        {}

        private class Aggregate : AggregateBase
        {
            public Aggregate(string id)
                : base(id)
            {}

            public void Command()
            {
                RaiseEvent(new EventThatDoesNotHaveAnApply());
            }
        }
    }
}