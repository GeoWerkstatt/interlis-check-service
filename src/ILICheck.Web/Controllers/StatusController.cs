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
        /// Action to get the status for the specified <paramref name="jobId"/>.
        /// </summary>
        /// <returns>JSON-formatted client application settings.</returns>
        [HttpGet("{jobId}")]
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
