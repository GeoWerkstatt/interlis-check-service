using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace ILICheck.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : Controller
    {
        private readonly IConfiguration configuration;
        public SettingsController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Action to get client application settings.
        /// </summary>
        /// <returns>JSON-formatted client application settings.</returns>
        [HttpGet]
        public IActionResult Get()
        {
            var acceptedFileTypes = ".xtf, .xml, .zip";
            var gpkgSupportEnabled = Environment.GetEnvironmentVariable("ENABLE_GPKG_VALIDATION", EnvironmentVariableTarget.Process) == "true";
            if (gpkgSupportEnabled) acceptedFileTypes += ", .gpkg";

            return new JsonResult(new
            {
                applicationName = Environment.GetEnvironmentVariable("CUSTOM_APP_NAME", EnvironmentVariableTarget.Process) ?? "INTERLIS Web-Check-Service",
                applicationVersion = Environment.GetEnvironmentVariable("ILICHECK_APP_VERSION", EnvironmentVariableTarget.Process) ?? "undefined",
                vendorLink = Environment.GetEnvironmentVariable("CUSTOM_VENDOR_LINK", EnvironmentVariableTarget.Process),
                ilivalidatorVersion = Environment.GetEnvironmentVariable("ILIVALIDATOR_VERSION", EnvironmentVariableTarget.Process) ?? "undefined",
                ili2gpkgVersion = Environment.GetEnvironmentVariable("ILI2GPKG_VERSION", EnvironmentVariableTarget.Process) ?? "undefined/not configured",
                acceptedFileTypes = acceptedFileTypes,
            });
        }
    }
}
