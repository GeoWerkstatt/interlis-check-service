using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using static ILICheck.Web.ValidatorHelper;
using UAParser;

namespace ILICheck.Web.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UploadController : Controller
    {
        private readonly ILogger<UploadController> logger;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IValidator validator;
        private readonly IFileProvider fileProvider;
        private readonly IValidatorService validatorService;

        public UploadController(ILogger<UploadController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IValidator validator, IFileProvider fileProvider, IValidatorService validatorService)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.validator = validator;
            this.fileProvider = fileProvider;
            this.validatorService = validatorService;

            this.fileProvider.Initialize(validator.Id);
        }

        /// <summary>
        /// Schedules a new job for the given transfer <paramref name="file"/>.
        /// </summary>
        /// <param name="version">The application programming interface (API) version.</param>
        /// <param name="file">The transfer or ZIP file to validate.</param>
        /// <remarks>
        /// ## Usage
        /// 
        /// ### CURL
        /// 
        /// ```bash
        /// curl -i -X POST -H "Content-Type: multipart/form-data" \
        ///   -F 'file=@example.xtf' https://example.com/api/v1/upload
        /// ```
        /// 
        /// ### JavaScript
        /// 
        /// ```bash
        /// import { createReadStream } from 'fs';
        /// import FormData from 'form-data';
        /// import fetch from 'node-fetch';
        /// 
        /// var form = new FormData();
        /// form.append('file', createReadStream('example.xtf'));
        /// const response = await fetch('https://example.com/api/v1/upload', {
        ///   method: 'POST',
        ///   body: form,
        /// });
        /// ```
        /// 
        /// ### Python
        /// 
        /// ```python
        /// import requests
        /// response = requests.post('https://example.com/api/v1/upload', files={'file':open('example.xtf')}).json()
        /// ```
        /// </remarks>
        /// <returns>Information for a newly created validation job.</returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "The validation job was successfully created and is now scheduled for execution.", typeof(UploadResponse), "application/json")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The server cannot process the request due to invalid or malformed request.", typeof(ProblemDetails), "application/json")]
        [SwaggerResponse(StatusCodes.Status413PayloadTooLarge, "The transfer file is too large. Max allowed request body size is 200 MB.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:DocumentationTextMustEndWithAPeriod", Justification = "Not applicable for code examples.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1028:CodeMustNotContainTrailingWhitespace", Justification = "Not applicable for code examples.")]
        public async Task<IActionResult> UploadAsync(ApiVersion version, IFormFile file)
        {
            if (file == null) return Problem($"Form data <{nameof(file)}> cannot be empty.", statusCode: StatusCodes.Status400BadRequest);
            var httpRequest = httpContextAccessor.HttpContext.Request;

            logger.LogInformation("Start uploading <{TransferFile}> to <{HomeDirectory}>", file.FileName, fileProvider.HomeDirectory);
            logger.LogInformation("Transfer file size: {ContentLength}", HttpUtility.HtmlEncode(httpRequest.ContentLength));
            logger.LogInformation("Start time: {Timestamp}", DateTime.Now);

            try
            {
                // Sanitize file name and save the file to the predefined home directory.
                var transferFile = Path.ChangeExtension(
                    Path.GetRandomFileName(),
                    file.FileName.GetSanitizedFileExtension(GetAcceptedFileExtensionsForUserUploads(configuration)));

                using (var stream = fileProvider.CreateFile(transferFile))
                {
                    await file.CopyToAsync(stream).ConfigureAwait(false);
                }

                logger.LogInformation("Successfully received file: {TransferFile}", file.FileName);

                // Add validation job to queue.
                await validatorService.EnqueueJobAsync(
                    validator.Id, cancellationToken => validator.ExecuteAsync(transferFile, cancellationToken));

                logger.LogInformation("Job with id <{JobId}> is scheduled for execution.", validator.Id);

                var location = new Uri(
                    string.Format(CultureInfo.InvariantCulture, "/api/v{0}/status/{1}", version.MajorVersion, validator.Id),
                    UriKind.Relative);

                return Created(location, new UploadResponse { JobId = validator.Id, StatusUrl = location });
            }
            catch (UnknownExtensionException ex)
            {
                logger.LogInformation(ex.Message);
                return Problem(ex.Message, statusCode: 400);
            }
        }
    }
}
