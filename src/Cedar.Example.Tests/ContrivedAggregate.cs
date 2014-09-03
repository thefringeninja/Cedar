namespace Cedar.Example.Tests
{
    using Cedar.Domain;
    using Cedar.Testing;
    using Xunit;

    public class ContrivedAggregate
    {
        class SomethingHappened { }
        class Aggregate : AggregateBase
        {
            private int _something = 0;

            public Aggregate(string id) : base(id)
            {
                
            }
            
            void Apply(SomethingHappened e)
            {
                _something++;
            }

            public void DoSomething()
            {
                RaiseEvent(new SomethingHappened());
            }

        }
        [Fact]
        public void Works()
        {
            Scenario.ForAggregate(() => new Aggregate("test"))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());
        }

        [Fact]
        public IScenario AlsoWorks()
        {
            return Scenario.ForAggregate(() => new Aggregate("test"))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());
        }
    }
}
