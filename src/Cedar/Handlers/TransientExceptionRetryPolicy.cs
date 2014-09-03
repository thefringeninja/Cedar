namespace Cedar.Handlers
{
    using System;
    using System.Diagnostics;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Logging;

    /// <summary>
    /// Represents a policy for handling a <see cref="TransientException"/>. 
    /// </summary>
    public class TransientExceptionRetryPolicy
    {
        private static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly TimeSpan _duration;
        private readonly TimeSpan _retryInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientExceptionRetryPolicy"/> class.
        /// </summary>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="duration">The duration.</param>
        /// <exception cref="System.ArgumentException">
        /// retryInterval
        /// or
        /// duration
        /// </exception>
        public TransientExceptionRetryPolicy(TimeSpan retryInterval, TimeSpan duration)
        {
            if (retryInterval.Ticks < 0)
            {
                throw new ArgumentException(Messages.NegativeRetryInterval, "retryInterval");
            }

            if (duration.Ticks < 0)
            {
                throw new ArgumentException(Messages.NegativeDuration, "duration");
            }

            _retryInterval = retryInterval;
            _duration = duration;
        }

        public static TransientExceptionRetryPolicy Indefinite(TimeSpan retryInterval)
        {
            return new TransientExceptionRetryPolicy(retryInterval, TimeSpan.MaxValue);
        }

        public static TransientExceptionRetryPolicy None()
        {
            return new TransientExceptionRetryPolicy(TimeSpan.Zero, TimeSpan.Zero);
        }

        public async Task Retry(Func<Task> operation, CancellationToken ct)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int attemptCount = 0;
            while (true)
            {
                TransientException transientException = null;
                try
                {
                    await operation();
                }
                catch (TransientException ex)
                {
                    Logger.WarnException(
                        Messages.TransientExceptionOccured.FormatWith(attemptCount),
                        ex);
                    transientException = ex;
                }
                if (transientException == null)
                {
                    break;
                }
                if (stopwatch.Elapsed < _duration)
                {
                    await Task.Delay(_retryInterval, ct);
                    attemptCount ++;
                    continue;
                }
                Logger.ErrorException(
                    Messages.TransientExceptionExceededDuration.FormatWith(attemptCount, _duration),
                    transientException);
                ExceptionDispatchInfo.Capture(transientException).Throw();
            }
        }
    }
}