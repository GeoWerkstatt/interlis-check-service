using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ILICheck.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DownloadController : Controller
    {
        private readonly ILogger<DownloadController> logger;
        private readonly IFileProvider fileProvider;

        public DownloadController(ILogger<DownloadController> logger, IFileProvider fileProvider)
        {
            this.logger = logger;
            this.fileProvider = fileProvider;
        }

        /// <summary>
        /// Gets the ilivalidator log file for the specified <paramref name="jobId"/> and <paramref name="logType"/>.
        /// </summary>
        /// <param name="jobId">The validation job identifier.</param>
        /// <param name="logType">The log type (log|xtf).</param>
        /// <response code="200">Returns the ilivalidator log file.</response>
        /// <response code="400">The server cannot process the request due to invalid or malformed request.</response>
        /// <response code="404">The log file for the requested <paramref name="jobId"/> cannot be found.</response>
        /// <returns>The ilivalidator log file.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Download(Guid jobId, LogType logType)
        {
            fileProvider.Initialize(jobId.ToString());

            try
            {
                logger.LogInformation("Log file (<{LogType}>) for job identifier <{JobId}> requested.", logType, jobId);
                return File(fileProvider.OpenText(fileProvider.GetLogFile(logType)).BaseStream, "text/xml; charset=utf-8");
            }
            catch (Exception)
            {
                Response.StatusCode = 404;
                return View("PageNotFound", "Die gesuchte Log-Datei wurde nicht gefunden. Möglicherweise wurde sie bereits gelöscht.");
            }
        }
    }
}
