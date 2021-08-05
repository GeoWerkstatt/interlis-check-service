using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ILICheck.Web.Controllers
{
    public class DownloadController : Controller
    {
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
            string directoryPath = Path.Combine(@".\Upload", connectionId);
            try
            {
                var logFiles = Directory.EnumerateFiles(directoryPath, "Ilivalidator_*", SearchOption.TopDirectoryOnly);
                var logFile = logFiles.Single();
                return PhysicalFile(Path.GetFullPath(logFile), "text/plain");
            }
            catch (Exception)
            {
                return NotFound("Requested logfile could not be found.");
            }
        }
    }
}
