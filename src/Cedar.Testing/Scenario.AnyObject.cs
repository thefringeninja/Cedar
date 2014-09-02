namespace Cedar.Testing
{
    using System;
    using FluentAssertions;

    public static partial class Scenario
    {
        public static IGiven<T> For<T>()
        {
            return new AnyObjectScenarioBuilder<T>();
        }

        private class AnyObjectScenarioBuilder<T> : IGiven<T>
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

            public void ThenShouldThrow<TException>(Func<TException, bool> equals = null) where TException : Exception
            {
                equals = equals ?? (_ => true);
                Action then = () => _when();

                _given();
                then.ShouldThrow<TException>()
                    .Which.Should().Match<TException>(ex => equals(ex));
            }

            public IThen<T> When(Func<T, T> when)
            {
                _when = () => _result = when(_instance);

                return this;
            }

            public IWhen<T> Given(T instance)
            {
                _given = () => _instance = instance;

                return this;
            }
        }

        public interface IGiven<T> : IWhen<T>
        {
            IWhen<T> Given(T instance);
        }

        public interface IThen<T>
        {
            void ThenShouldEqual(T other);
            void ThenShouldThrow<TException>(Func<TException, bool> equals = null) where TException : Exception;
        }

        public interface IWhen<T> : IThen<T>
        {
            IThen<T> When(Func<T, T> when);
        }
    }
}