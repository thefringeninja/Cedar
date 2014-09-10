namespace Cedar.Handlers
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class TransientExceptionRetryPolicyTests
    {
        [Fact]
        public void Can_not_create_with_a_negative_retry_interval()
        {
            Action act = () => new TransientExceptionRetryPolicy(TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(1));

            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Can_not_create_with_a_negative_duration()
        {
            Action act = () => new TransientExceptionRetryPolicy(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(-1));

            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task When_transient_exception_thrown_once_then_should_retry()
        {
            var sut = new TransientExceptionRetryPolicy(TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1));
            int count = 0;
            var tcs = new TaskCompletionSource<bool>();

            await sut.Retry(() =>
            {
                count++;
                if (count == 2)
                {
                    tcs.SetResult(true);
                    return Task.FromResult(0);
                }
                throw new TransientException();
            },  CancellationToken.None);

            await tcs.Task;

            count.Should().Be(2);
        }

        [Fact]
        public void When_non_transient_exception_thrown_then_should_retry()
        {
            var sut = new TransientExceptionRetryPolicy(TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(10));

            Func<Task> act = () => sut.Retry(() => { throw new Exception(); }, CancellationToken.None);

            act.ShouldThrow<Exception>();
        }

        [Fact]
        public void When_transient_exception_thrown_indefinitely_then_should_repeatedly_retry_until_duration_and_then_throw()
        {
            var duration = TimeSpan.FromSeconds(1);
            var stopwatch = Stopwatch.StartNew();
            var sut = new TransientExceptionRetryPolicy(TimeSpan.FromMilliseconds(1), duration);
            int count = 0;

            Func<Task> act = () => sut.Retry(() =>
            {
                count++;
                throw new TransientException();
            }, CancellationToken.None);

            act.ShouldThrow<TransientException>();
            count.Should().BeGreaterThan(1);
            stopwatch.Elapsed.Should().BeGreaterThan(duration);
        }

        [Fact]
        public async Task Transient_policy_none_should_not_retry()
        {
            var sut = TransientExceptionRetryPolicy.None();
            int count = 0;

            Func<Task> act = () => sut.Retry(() =>
            {
                count++;
                if (count == 1)
                {
                    throw new TransientException();
                }
                return Task.FromResult(0);
            }, CancellationToken.None);

            act.ShouldThrow<TransientException>();
            count.Should().Be(1);
        }
    }
}