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

        public UploadController(ILogger<UploadController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IValidator validator)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.validator = validator;
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
        /// <response code="400">The server cannot precess the request due to invalid or malformed request.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:DocumentationTextMustEndWithAPeriod", Justification = "Not applicable for code examples.")]
        public async Task<IActionResult> Post(IFormFile file)
        {
            var httpRequest = httpContextAccessor.HttpContext.Request;

            // TODO: Additional argument validation required? (eg. file != null && file.length > 0)

            // Create a unique validation job identification
            var jobId = Guid.NewGuid().ToString();

            // TODO: Create folder for validation job files
            // TODO: Sanitize file/folder names (save to disk/logging)
            // var uploadFolderPath = configuration.GetUploadPathForSession(jobId);
            var uploadFolderPath = Path.Combine(@"C:\Temp", jobId);
            var uploadFileName = Path.Combine(uploadFolderPath, Path.GetRandomFileName());
            Directory.CreateDirectory(uploadFolderPath);

            // Log some information
            logger.LogInformation("Start uploading <{fileName}> to <{folder}>", file.FileName, uploadFolderPath);
            logger.LogInformation("Transfer file size: {contentLength}", httpRequest.ContentLength);
            logger.LogInformation("Start time: {timestamp}", DateTime.Now);

            // TODO: Handle exception while uploading files
            // logger.LogWarning("Upload aborted, no files data in the request.");
            // return BadRequest("Es wurde keine Datei hochgeladen.");

            // Save the file to disk
            using (var stream = System.IO.File.Create(uploadFileName))
            {
                await file.CopyToAsync(stream);
            }

            logger.LogInformation("Successfully received file: {timestamp}", DateTime.Now);

            // TODO: Schedule job/ Add to job queue.
            _ = validator.ValidateAsync(jobId, uploadFolderPath);
            logger.LogInformation("Job with id <{id}> is scheduled for execution.", jobId);

            // TODO: Return HTTP 201 instead of HTTP 200
            return new JsonResult(new
            {
                jobId,
                statusUrl = string.Format("{0}/{1}", httpRequest.Path.Value, jobId),
            });
        }
    }
}
