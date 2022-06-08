using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ILICheck.Web
{
    /// <summary>
    /// Schedules validation jobs and provides access to status information for a specific job.
    /// </summary>
    public class ValidatorService : BackgroundService, IValidatorService
    {
        private readonly ILogger<ValidatorService> logger;
        private readonly Channel<(string Id, Func<CancellationToken, Task> Task)> queue;
        private readonly ConcurrentDictionary<string, (Status Status, string StatusMessage)> jobs = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorService"/> class.
        /// </summary>
        public ValidatorService(ILogger<ValidatorService> logger)
        {
            this.logger = logger;
            queue = Channel.CreateUnbounded<(string, Func<CancellationToken, Task>)>();
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await queue.Reader.ReadAllAsync(stoppingToken).ParallelForEachAsync(async item =>
            {
                try
                {
                    UpdateJobStatus(item.Id, Status.Processing, "Die Datei wird validiert...");
                    await item.Task(stoppingToken);
                    UpdateJobStatus(item.Id, Status.Completed, "Die Daten sind modellkonform.");
                }
                catch (UnknownExtensionException ex)
                {
                    UpdateJobStatus(item.Id, Status.CompletedWithErrors, $"Die Dateiendung {ex.FileExtension} ist nicht erlaubt.", ex.Message);
                }
                catch (MultipleTransferFileFoundException ex)
                {
                    UpdateJobStatus(item.Id, Status.CompletedWithErrors, $"Es wurden mehrere Transferdateien mit der Dateiendung <{ex.FileExtension}> gefunden.", ex.Message);
                }
                catch (TransferFileNotFoundException ex)
                {
                    UpdateJobStatus(item.Id, Status.CompletedWithErrors, "Es konnte keine gültige Transferdatei gefunden werden.", ex.Message);
                }
                catch (GeoPackageException ex)
                {
                    UpdateJobStatus(item.Id, Status.CompletedWithErrors, "Die Modellnamen konnten nicht aus der GeoPackage Datenbank gelesen werden.", ex.Message);
                }
                catch (Exception ex)
                {
                    var traceId = Guid.NewGuid();
                    UpdateJobStatus(item.Id, Status.Failed, $"Unbekannter Fehler. Fehler-Id: <{traceId}>");
                    logger.LogError(ex, "Unhandled exception TraceId: <{TraceId}> Message: <{ErrorMessage}>", traceId, ex.Message);
                }
            }, stoppingToken);
        }

        /// <inheritdoc/>
        public async Task EnqueueJobAsync(string jobId, Func<CancellationToken, Task> action)
        {
            UpdateJobStatus(jobId, Status.Enqueued, "Die Validierung wird vorbereitet...");
            await queue.Writer.WriteAsync((jobId, action));
        }

        /// <inheritdoc/>
        public (Status Status, string StatusMessage) GetJobStatusOrDefault(string jobId) => jobs[jobId];

        /// <summary>
        /// Adds or updates the status for the given <paramref name="jobId"/>.
        /// </summary>
        /// <param name="jobId">The job identifier to be added or whose value should be updated.</param>
        /// <param name="status">The status.</param>
        /// <param name="statusMessage">The status message.</param>
        /// <param name="logMessage">Optional info log message.</param>
        private void UpdateJobStatus(string jobId, Status status, string statusMessage, string logMessage = null)
        {
            jobs[jobId] = (status, statusMessage);
            if (!string.IsNullOrEmpty(logMessage)) logger.LogInformation(logMessage);
        }
    }
}
