using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cedar
{
    using System.Threading;

    public static class TaskExtensions
    {
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan delay)
        {
            var cts = new CancellationTokenSource();

            var completedTask = await Task.WhenAny(task, Task.Delay(delay, cts.Token));

            if(completedTask != task)
            {
                throw new TimeoutException("The operation has timed out.");
            }

            cts.Cancel();

            return await task;
        }

        public static async Task WithTimeout(this Task task, TimeSpan delay)
        {
            var cts = new CancellationTokenSource();

            var completedTask = await Task.WhenAny(task, Task.Delay(delay, cts.Token));

            if (completedTask != task)
            {
                throw new TimeoutException("The operation has timed out.");
            }

            cts.Cancel();

            await task;
        }
    }
}
