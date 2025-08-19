﻿using Geowerkstatt.Ilicop.Web.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using static Geowerkstatt.Ilicop.Web.ValidatorHelper;

namespace Geowerkstatt.Ilicop.Web.Controllers
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
        /// Gets the application settings.
        /// </summary>
        /// <returns>JSON-formatted client application settings.</returns>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "The the application settings which can be used to configure a client.", typeof(SettingsResponse), "application/json")]
        public IActionResult GetSettings()
        {
            logger.LogTrace("Application configuration requested.");

            return Ok(new SettingsResponse
            {
                ApplicationName = configuration.GetValue<string>("CUSTOM_APP_NAME") ?? "INTERLIS Web-Check-Service",
                ApplicationVersion = configuration.GetValue<string>("ILICOP_APP_VERSION") ?? "undefined",
                VendorLink = configuration.GetValue<string>("CUSTOM_VENDOR_LINK"),
                IlivalidatorVersion = configuration.GetValue<string>("ILIVALIDATOR_VERSION") ?? "undefined",
                Ili2gpkgVersion = configuration.GetValue<string>("ILI2GPKG_VERSION") ?? "undefined/not configured",
                AcceptedFileTypes = GetAcceptedFileExtensionsForUserUploads(configuration).JoinNonEmpty(", "),
            });
        }
    }
}
