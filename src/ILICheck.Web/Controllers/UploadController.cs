using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace ILICheck.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : Controller
    {
        private readonly ILogger<UploadController> logger;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IValidator validator;
        private readonly IFileProvider fileProvider;

        public UploadController(ILogger<UploadController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IValidator validator, IFileProvider fileProvider)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.validator = validator;
            this.fileProvider = fileProvider;

            this.fileProvider.Initialize(validator.Id);
        }

        /// <summary>
        /// Creates a new validation job for the given transfer <paramref name="file"/>
        /// A compressed <paramref name="file"/> (.zip) containing additional models and
        /// catalogues is also supported.
        /// </summary>
        /// <param name="file">The transfer or ZIP file to validate.</param>
        /// <remarks>
        /// Sample request:
        /// curl -i -X POST -H "Content-Type: multipart/form-data" \
        ///   -F 'file=@example.xtf' http://example.com/api/upload
        /// </remarks>
        /// <returns>Information for a newly created validation job.</returns>
        /// <response code="201">The validation job was successfully created and is now scheduled for execution.</response>
        /// <response code="400">The server cannot process the request due to invalid or malformed request.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:DocumentationTextMustEndWithAPeriod", Justification = "Not applicable for code examples.")]
        public async Task<IActionResult> Post(IFormFile file)
        {
            var httpRequest = httpContextAccessor.HttpContext.Request;

            logger.LogInformation("Start uploading <{fileName}> to <{folder}>", file.FileName, fileProvider.HomeDirectory);
            logger.LogInformation("Transfer file size: {contentLength}", httpRequest.ContentLength);
            logger.LogInformation("Start time: {timestamp}", DateTime.Now);

            // Sanitize file name and save the file to disk
            var transferFile = Path.ChangeExtension(Path.GetRandomFileName(), configuration.GetSanitizedFileExtension(file.FileName));
            using (var stream = fileProvider.CreateFile(transferFile))
            {
                await file.CopyToAsync(stream);
            }

            logger.LogInformation("Successfully received file: {timestamp}", DateTime.Now);

            _ = validator.ValidateAsync(transferFile);
            logger.LogInformation("Job with id <{id}> is scheduled for execution.", validator.Id);

            Response.StatusCode = 201;
            return new JsonResult(new
            {
                validator.Id,
                statusUrl = string.Format("{0}/{1}", httpRequest.Path.Value, validator.Id),
            });
        }
    }
}
