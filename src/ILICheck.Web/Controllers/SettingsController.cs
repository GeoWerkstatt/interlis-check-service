using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static ILICheck.Web.ValidatorHelper;

namespace ILICheck.Web.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> logger;
        private readonly IConfiguration configuration;

        public SettingsController(ILogger<SettingsController> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        /// <summary>
        /// Action to get client application settings.
        /// </summary>
        /// <returns>JSON-formatted client application settings.</returns>
        [HttpGet]
        public IActionResult GetSettings()
        {
            logger.LogTrace("Application configuration requested.");

            return Ok(new
            {
                applicationName = configuration.GetValue<string>("CUSTOM_APP_NAME") ?? "INTERLIS Web-Check-Service",
                applicationVersion = configuration.GetValue<string>("ILICHECK_APP_VERSION") ?? "undefined",
                vendorLink = configuration.GetValue<string>("CUSTOM_VENDOR_LINK"),
                ilivalidatorVersion = configuration.GetValue<string>("ILIVALIDATOR_VERSION") ?? "undefined",
                ili2gpkgVersion = configuration.GetValue<string>("ILI2GPKG_VERSION") ?? "undefined/not configured",
                acceptedFileTypes = GetAcceptedFileExtensionsForUserUploads(configuration).JoinNonEmpty(", "),
            });
        }
    }
}
