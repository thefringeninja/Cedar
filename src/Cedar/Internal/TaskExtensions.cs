// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class TaskExtensions
    {
        internal static ConfiguredTaskAwaitable<T> NotOnCapturedContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        internal static ConfiguredTaskAwaitable NotOnCapturedContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        internal static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan delay)
        {
            var cts = new CancellationTokenSource();
            Task completedTask = await Task.WhenAny(task, Task.Delay(delay, cts.Token));
            if (completedTask != task)
            {
                throw new TimeoutException("The operation has timed out.");
            }
            cts.Cancel();
            return await task.NotOnCapturedContext();
        }

        internal static async Task WithTimeout(this Task task, TimeSpan delay)
        {
            var cts = new CancellationTokenSource();
            Task completedTask = await Task.WhenAny(task, Task.Delay(delay, cts.Token));
            if (completedTask != task)
            {
                throw new TimeoutException("The operation has timed out.");
            }
            cts.Cancel();
            await task.NotOnCapturedContext();
        }
    }
}
