using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace ILICheck.Web.Controllers
{
    public class DownloadController : Controller
    {
        private readonly IConfiguration configuration;
        public DownloadController(IConfiguration configuration)
        {
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
            var connectionId = request.Query["connectionId"][0];
            var fileExtension = request.Query["fileExtension"][0];
            var directoryPath = configuration.GetUploadPathForSession(connectionId);
            try
            {
                var logFiles = Directory.EnumerateFiles(directoryPath, "ilivalidator_*", SearchOption.TopDirectoryOnly);
                var logFile = logFiles
                    .Where(file => Path.GetExtension(file) == fileExtension)
                    .Single();

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
