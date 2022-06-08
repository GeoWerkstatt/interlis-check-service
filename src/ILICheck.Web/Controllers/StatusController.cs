using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace ILICheck.Web.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class StatusController : Controller
    {
        private readonly ILogger<StatusController> logger;
        private readonly IValidatorService validatorService;

        public StatusController(ILogger<StatusController> logger, IValidatorService validatorService)
        {
            this.logger = logger;
            this.validatorService = validatorService;
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

            var downloadLogUrlTemplate = "/api/v{0}/download?jobId={1}&logType={2}";
            Uri GetDownloadLogUrl(LogType logType) =>
                new (string.Format(
                    CultureInfo.InvariantCulture,
                    downloadLogUrlTemplate,
                    version.MajorVersion,
                    jobId,
                    logType.ToString().ToLowerInvariant()),
                    UriKind.Relative);

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
                logUrl = GetDownloadLogUrl(LogType.Log),
                xtfLogUrl = GetDownloadLogUrl(LogType.Xtf),
            });
        }
    }
}
