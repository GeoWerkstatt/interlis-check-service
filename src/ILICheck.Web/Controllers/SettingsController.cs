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
            return new JsonResult(new
            {
                applicationName = Environment.GetEnvironmentVariable("ILICHECK_APP_NAME", EnvironmentVariableTarget.Process) ?? "INTERLIS Web-Check-Service",
                applicationTagline = Environment.GetEnvironmentVariable("ILICHECK_APP_TAGLINE", EnvironmentVariableTarget.Process) ?? "Online-Validierung von INTERLIS Daten",
                applicationLogo = Environment.GetEnvironmentVariable("ILICHECK_APP_LOGO", EnvironmentVariableTarget.Process) ?? "app.png",
                applicationFavicon = Environment.GetEnvironmentVariable("ILICHECK_APP_FAVICON", EnvironmentVariableTarget.Process) ?? "favicon.ico",
                applicationVersion = Environment.GetEnvironmentVariable("ILICHECK_APP_VERSION", EnvironmentVariableTarget.Process) ?? "undefined",
                vendorLink = Environment.GetEnvironmentVariable("ILICHECK_VENDOR_LINK", EnvironmentVariableTarget.Process),
                vendorLogo = Environment.GetEnvironmentVariable("ILICHECK_VENDOR_LOGO", EnvironmentVariableTarget.Process) ?? "vendor.png",
                imprint = Environment.GetEnvironmentVariable("ILICHECK_IMPRINT", EnvironmentVariableTarget.Process) ?? "impressum.md",
                privacy = Environment.GetEnvironmentVariable("ILICHECK_PRIVACY", EnvironmentVariableTarget.Process) ?? "datenschutz.md",
                guide = Environment.GetEnvironmentVariable("ILICHECK_GUIDE", EnvironmentVariableTarget.Process) ?? "info-hilfe.md",
                termsOfUse = Environment.GetEnvironmentVariable("ILICHECK_TERMS_OF_USE", EnvironmentVariableTarget.Process) ?? "nutzungsbestimmungen.md",
                quickstart = Environment.GetEnvironmentVariable("ILICHECK_QUICKSTART", EnvironmentVariableTarget.Process) ?? "quickstart.txt",
                ilivalidatorVersion = Environment.GetEnvironmentVariable("ILIVALIDATOR_VERSION", EnvironmentVariableTarget.Process) ?? "undefined",
                htmlMetaDescription = Environment.GetEnvironmentVariable("ILICHECK_HTML_META_DESCRIPTION", EnvironmentVariableTarget.Process),
                htmlMetaKeywords = Environment.GetEnvironmentVariable("ILICHECK_HTML_META_KEYWORDS", EnvironmentVariableTarget.Process),
                htmlMetaAuthor = Environment.GetEnvironmentVariable("ILICHECK_HTML_META_AUTHOR", EnvironmentVariableTarget.Process),
                htmlMetaRobot = Environment.GetEnvironmentVariable("ILICHECK_HTML_META_ROBOT", EnvironmentVariableTarget.Process) ?? "index, follow",
            });
        }
    }
}
