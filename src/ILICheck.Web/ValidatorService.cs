using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ILICheck.Web
{
    /// <summary>
    /// Schedules validation jobs.
    /// </summary>
    public class ValidatorService : BackgroundService, IValidatorService
    {
        private readonly Channel<(string Id, Func<CancellationToken, Task> Task)> queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorService"/> class.
        /// </summary>
        public ValidatorService()
        {
            queue = Channel.CreateUnbounded<(string, Func<CancellationToken, Task>)>();
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await queue.Reader
                .ReadAllAsync(stoppingToken)
                .ParallelForEachAsync(item => item.Task(stoppingToken), stoppingToken);
        }

        /// <inheritdoc/>
        public async Task EnqueueJobAsync(string jobId, Func<CancellationToken, Task> action)
        {
            await queue.Writer.WriteAsync((jobId, action));
        }
    }
}
