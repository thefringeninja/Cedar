namespace Cedar.Example.Tests
{
    using System.Threading.Tasks;
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
        public async Task Passes()
        {
            await Scenario.ForAggregate(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());
        }

        [Fact]
        public async Task<ScenarioResult> AlsoPasses()
        {
            return await Scenario.ForAggregate(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());
        }

        [Fact]
        public async Task DoesNotPass()
        {
            await Scenario.ForAggregate(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .ThenNothingHappened();
        }

        [Fact]
        public async Task<ScenarioResult> AlsoDoesNotPass()
        {
            return await Scenario.ForAggregate(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .ThenNothingHappened();
        }

    }
}
