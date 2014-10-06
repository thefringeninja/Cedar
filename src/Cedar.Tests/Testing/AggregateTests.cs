namespace Cedar.Testing
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Domain;
    using Xunit;

    public class AggregateTests
    {
        class SomethingHappened
        {
            public override string ToString()
            {
                return "Something happened.";
            }
        }

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

        class BuggyAggregate : AggregateBase
        {
            public BuggyAggregate(string id) : base(id)
            {}

            public void DoSomething()
            {
                throw new InvalidOperationException();
            }
        }

        class ReallyBuggyAggregate : AggregateBase
        {
            public ReallyBuggyAggregate(string id)
                : base(id)
            {
                throw new InvalidOperationException();
            }

            public void DoSomething()
            {
            }
        }

        class ConstructorBehaviorAggregate : AggregateBase
        {
            public ConstructorBehaviorAggregate(Guid id)
                : base(id.ToString())
            {
                RaiseEvent(new SomethingHappened());
            }
            protected ConstructorBehaviorAggregate(string id) : base(id)
            {}

            void Apply(SomethingHappened e) { }
        }
        [Fact]
        public async Task a_passing_aggregate_scenario_should()
        {
            var result = await Scenario.ForAggregate(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_passing_aggregate_with_events_raised_in_the_constructor_should()
        {
            var result = await Scenario.ForAggregate<ConstructorBehaviorAggregate>()
                .When(() => new ConstructorBehaviorAggregate(Guid.Empty))
                .Then(new SomethingHappened());

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_passing_aggregate_scenario_with_no_given_should()
        {
            var result = await Scenario.ForAggregate(id => new Aggregate(id))
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task an_aggregate_throwing_an_exception_should()
        {
            var result = await Scenario.ForAggregate(id => new BuggyAggregate(id))
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());

            Assert.False(result.Passed);
            Assert.IsType<ScenarioException>(result.Results);
        }


        [Fact]
        public async Task an_aggregate_throwing_an_exception_in_its_constructor_should()
        {
            var result = await Scenario.ForAggregate(id => new ReallyBuggyAggregate(id))
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());

            Assert.False(result.Passed);
            Assert.IsType<ScenarioException>(result.Results);
        }


        [Fact]
        public async Task an_aggregate_throwing_an_expected_exception_should()
        {
            var result = await Scenario.ForAggregate(id => new BuggyAggregate(id))
                .When(a => a.DoSomething())
                .ThenShouldThrow<InvalidOperationException>();

            Assert.True(result.Passed);
            Assert.IsType<InvalidOperationException>(result.Results);
        }

    }
}
