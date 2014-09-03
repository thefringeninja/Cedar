namespace Cedar.Testing
{
    using System;
    using FluentAssertions;

    public static partial class Scenario
    {
        public static Any.Given<T> For<T>()
        {
            return new Any.ScenarioBuilder<T>();
        }

        public static class Any
        {
            public interface Given<T> : When<T>
            {
                When<T> Given(T instance);
            }

            public interface When<T> : Then<T>
            {
                Then<T> When(Func<T, T> when);
            }

            public interface Then<T>
            {
                void ThenShouldEqual(T other);
                void ThenShouldThrow<TException>(Func<TException, bool> equals = null) where TException : Exception;
            }

            internal class ScenarioBuilder<T> : Given<T>
            {
                private Action _given = () => { };
                private T _instance;
                private T _result;

                private Action _when;

                public void ThenShouldEqual(T other)
                {
                    _given();
                    _when();
                    _result.Should().Be(other);
                }

                public void ThenShouldThrow<TException>(Func<TException, bool> equals = null)
                    where TException : Exception
                {
                    equals = equals ?? (_ => true);
                    Action then = () => _when();

                    _given();
                    then.ShouldThrow<TException>()
                        .Which.Should().Match<TException>(ex => equals(ex));
                }

                public Then<T> When(Func<T, T> when)
                {
                    _when = () => _result = when(_instance);

                    return this;
                }

                public When<T> Given(T instance)
                {
                    _given = () => _instance = instance;

                    return this;
                }
            }
        }
    }
}