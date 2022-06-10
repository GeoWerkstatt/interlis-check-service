using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;

namespace ILICheck.Web.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class StatusController : Controller
    {
        private readonly ILogger<StatusController> logger;
        private readonly IValidatorService validatorService;
        private readonly IFileProvider fileProvider;

        public StatusController(ILogger<StatusController> logger, IValidatorService validatorService, IFileProvider fileProvider)
        {
            this.logger = logger;
            this.validatorService = validatorService;
            this.fileProvider = fileProvider;
        }

        /// <summary>
        /// Gets the status information for the specified <paramref name="jobId"/>.
        /// </summary>
        /// <param name="version">The application programming interface (API) version.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>The status information for the specified <paramref name="jobId"/>.</returns>
        /// <response code="200">The job with the specified <paramref name="jobId"/> was found.</response>
        /// <response code="400">The server cannot process the request due to invalid or malformed request.</response>
        /// <response code="404">The job with the specified <paramref name="jobId"/> cannot be found.</response>
        [HttpGet("{jobId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetStatus(ApiVersion version, Guid jobId)
        {
            logger.LogTrace("Status for job id <JobId> requested.", jobId);

            fileProvider.Initialize(jobId.ToString());

            var job = validatorService.GetJobStatusOrDefault(jobId.ToString());
            if (job == default)
            {
                return Problem($"No job information available for job id <{jobId}>", statusCode: StatusCodes.Status404NotFound);
            }

            return Ok(new
            {
                jobId,
                status = job.Status,
                statusMessage = job.StatusMessage,
                logUrl = GetLogDownloadUrl(version, jobId.ToString(), LogType.Log),
                xtfLogUrl = GetLogDownloadUrl(version, jobId.ToString(), LogType.Xtf),
            });
        }

        /// <summary>
        /// Gets the log download URL for the specified <paramref name="logType"/>.
        /// </summary>
        /// <param name="version">The application programming interface (API) version.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="logType">The log type (log|xtf).</param>
        /// <returns>The log download URL if the log file exists; otherwise, <c>null</c>.</returns>
        internal Uri GetLogDownloadUrl(ApiVersion version, string jobId, LogType logType)
        {
            try
            {
                _ = fileProvider.GetLogFile(logType);
            }
            catch (FileNotFoundException)
            {
                return null;
            }

            var downloadLogUrlTemplate = "/api/v{0}/download?jobId={1}&logType={2}";
            return new Uri(string.Format(
                CultureInfo.InvariantCulture,
                downloadLogUrlTemplate,
                version.MajorVersion,
                jobId,
                logType.ToString().ToLowerInvariant()),
                UriKind.Relative);
        }
    }
}
