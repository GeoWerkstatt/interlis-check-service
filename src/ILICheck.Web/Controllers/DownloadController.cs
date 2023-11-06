using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Web;

namespace ILICheck.Web.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
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
        /// <param name="jobId" example="2e71ae96-e6ad-4b67-b817-f09412d09a2c">The job identifier.</param>
        /// <param name="logType">The log type to download.</param>
        /// <returns>The ilivalidator log file.</returns>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status201Created, "Returns the ilivalidator log file.", ContentTypes = new[] { "text/xml; charset=utf-8" })]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The server cannot process the request due to invalid or malformed request.", typeof(ProblemDetails), new[] { "application/json" })]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The log file for the requested jobId cannot be found.", ContentTypes = new[] { "application/json" })]
        public IActionResult Download(Guid jobId, LogType logType)
        {
            fileProvider.Initialize(jobId);

            try
            {
                logger.LogInformation("Log file (<{LogType}>) for job identifier <{JobId}> requested.", HttpUtility.HtmlEncode(logType), jobId);
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
