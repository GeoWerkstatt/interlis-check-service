using System;
using System.Threading;
using System.Threading.Tasks;

namespace ILICheck.Web
{
    /// <summary>
    /// Provides methods to schedule validation jobs and access job status information.
    /// </summary>
    public interface IValidatorService
    {
        /// <summary>
        /// Asynchronously enqueues and executes the <paramref name="action"/> specified.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns></returns>
        Task EnqueueJobAsync(string jobId, Func<CancellationToken, Task> action);

        /// <summary>
        /// Gets the <see cref="Status"/> for the given <paramref name="jobId"/>.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>The <see cref="Status"/> for the given <paramref name="jobId"/> if the job exists; otherwise, <c>default</c>.</returns>
        (Status Status, string StatusMessage) GetJobStatusOrDefault(string jobId);
    }
}
