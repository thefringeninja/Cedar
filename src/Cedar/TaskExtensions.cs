namespace Cedar
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Internal;

    public static class TaskExtensions
    {
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan delay)
        {
            var cts = new CancellationTokenSource();
            Task completedTask = await Task.WhenAny(task, Task.Delay(delay, cts.Token));
            if(completedTask != task)
            {
                throw new TimeoutException("The operation has timed out.");
            }
            cts.Cancel();
            return await task.NotOnCapturedContext();
        }

        public static async Task WithTimeout(this Task task, TimeSpan delay)
        {
            var cts = new CancellationTokenSource();
            Task completedTask = await Task.WhenAny(task, Task.Delay(delay, cts.Token));
            if(completedTask != task)
            {
                throw new TimeoutException("The operation has timed out.");
            }
            cts.Cancel();
            await task.NotOnCapturedContext();
        }
    }
}