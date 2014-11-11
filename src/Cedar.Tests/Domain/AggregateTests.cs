using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cedar.Domain
{
    using FluentAssertions;
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

        class SomethingElseHappened
        {
            public override string ToString()
            {
                return "Something else happened.";
            }
        }

        class Aggregate : AggregateBase
        {
            private int _something = 0;

            public Aggregate(string id)
                : base(id)
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

            public void DoSomethingElse()
            {
                RaiseEvent(new SomethingElseHappened());
            }

        }

        [Fact]
        public void should_not_require_an_explicit_handler()
        {
            var aggregate = new Aggregate("id");

            aggregate.DoSomething();
            aggregate.DoSomethingElse();

            aggregate.Version.Should().Be(2);
        }
    }
}
