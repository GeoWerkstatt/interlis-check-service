using Geowerkstatt.Ilicop.Models;
using Geowerkstatt.Ilicop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace Geowerkstatt.Ilicop.Web.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> logger;
        private readonly IProfileService profileService;

        public ProfileController(ILogger<ProfileController> logger, IProfileService profileService)
        {
            this.logger = logger;
            this.profileService = profileService;
        }

        /// <summary>
        /// Gets all profiles.
        /// </summary>
        /// <returns>List of profiles.</returns>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "All existing profiles.", typeof(IEnumerable<Profile>), "application/json")]
        public IActionResult GetAll()
        {
            logger.LogTrace("Getting all profiles.");

            return Ok(profileService.GetProfiles());
        }
    }
}
