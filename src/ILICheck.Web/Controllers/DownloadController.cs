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
            var uploadPathFormat = configuration.GetSection("Upload")["PathFormat"];
            var directoryPath = uploadPathFormat.Replace("{Name}", connectionId);
            try
            {
                var logFiles = Directory.EnumerateFiles(directoryPath, "Ilivalidator_*", SearchOption.TopDirectoryOnly);
                var logFile = logFiles
                    .Where(file => Path.GetExtension(file) == fileExtension)
                    .Single();

                return PhysicalFile(Path.GetFullPath(logFile), "text/plain");
            }
            catch (Exception)
            {
                return NotFound("Requested logfile could not be found.");
            }
        }
    }
}
