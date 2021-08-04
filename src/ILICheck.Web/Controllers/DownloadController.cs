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
        /// <returns>A <see cref="PhysicalFileResult"/>.</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public PhysicalFileResult Download()
        {
            string path = @".\Download";
            path = Path.Combine(Path.GetFullPath(path), "Mock_ilivalidator_log.xtf");
            return PhysicalFile(path, "text/plain");
        }
    }
}
