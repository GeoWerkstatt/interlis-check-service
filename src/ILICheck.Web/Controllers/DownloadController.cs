using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace ILICheck.Web.Controllers
{
    public class DownloadController : Controller
    {
        private readonly ILogger<DownloadController> logger;
        private readonly IConfiguration configuration;

        public DownloadController(ILogger<DownloadController> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        /// <summary>
        /// Action to download log file from a directory.
        /// </summary>
        /// <returns>A <see cref="PhysicalFileResult"/> if successful, a <see cref="NotFoundResult"/> otherwise.</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Download()
        {
            var request = HttpContext.Request;
            var fileExtension = request.Query["fileExtension"][0];

            // Attention! This breaks downloading logs!
            // TODO use job id to identify assets for download
            var connectionId = Guid.NewGuid().ToString();
            var directoryPath = configuration.GetUploadPathForSession(connectionId);
            try
            {
                var logFiles = Directory.EnumerateFiles(directoryPath, "ilivalidator_*", SearchOption.TopDirectoryOnly);
                var logFile = logFiles
                    .Where(file => Path.GetExtension(file) == fileExtension)
                    .Single();

                logger.LogInformation("XTF log file for connection id <{connectionId}> requested.", connectionId);
                return File(System.IO.File.ReadAllBytes(logFile), "text/xml; charset=utf-8");
            }
            catch (Exception)
            {
                Response.StatusCode = 404;
                return View("PageNotFound", "Die gesuchte XTF-Log-Datei wurde leider nicht gefunden. Möglicherweise wurde diese bereits gelöscht.");
            }
        }
    }
}
