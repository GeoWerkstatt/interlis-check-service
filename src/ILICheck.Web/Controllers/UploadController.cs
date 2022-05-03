using ILICheck.Web.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ILICheck.Web.Controllers
{
    public class UploadController : Controller
    {
        private readonly ILogger<UploadController> logger;
        private readonly IConfiguration configuration;
        private readonly IValidator validator;

        public string UploadFolderPath { get; set; }
        public string UploadFilePath { get; set; }

        public UploadController(ILogger<UploadController> logger, IConfiguration configuration, IValidator validator)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.validator = validator;
        }

        /// <summary>
        /// Action to upload files to a directory.
        /// </summary>
        /// <returns>A <see cref="Task"/> of type <see cref="IActionResult"/>.</returns>
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> UploadAsync()
        {
            var request = HttpContext.Request;
            var connectionId = Guid.NewGuid().ToString();
            var fileName = request.Query["fileName"][0];
            var deleteTransferFile = string.Equals(
                Environment.GetEnvironmentVariable("DELETE_TRANSFER_FILES", EnvironmentVariableTarget.Process),
                "true",
                StringComparison.OrdinalIgnoreCase);

            MakeUploadFolder(connectionId);

            logger.LogInformation("Start uploading: {fileName}", fileName);
            logger.LogInformation("File size: {contentLength}", request.ContentLength);
            logger.LogInformation("Start time: {timestamp}", DateTime.Now);
            logger.LogInformation("Delete transfer file(s) after validation: {deleteTransferFile}", deleteTransferFile);

            // TODO: Handle exception while uploading file...
            await UploadToDirectoryAsync(request);

            // Fire and forget
            // Later on Hangfire is used to schedule job
            _ = validator.ValidateAsync(connectionId, deleteTransferFile, UploadFilePath);

            logger.LogInformation("Successfully received file: {timestamp}", DateTime.Now);

            // TODO: Return upload result, default HTTP 201
            return new JsonResult(new
            {
                jobId = connectionId,
                statusUrl = "api/v1/status/ff11cb95-1a91-4fea-ba86-2ed5c35a0d56",
            });
        }

        private void MakeUploadFolder(string connectionId)
        {
            UploadFolderPath = configuration.GetUploadPathForSession(connectionId);
            Directory.CreateDirectory(UploadFolderPath);
        }

        private async Task<IActionResult> UploadToDirectoryAsync(HttpRequest request)
        {
            logger.LogInformation("Uploading file");
            if (!request.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                logger.LogWarning("Upload aborted, unsupported media type.");
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var contentDisposition);

                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    var timestamp = DateTime.Now.ToString("yyyy_MM_d_HHmmss");
                    var uploadFileName = $"{timestamp}_{contentDisposition.FileName.Value}";
                    UploadFilePath = Path.Combine(UploadFolderPath, uploadFileName);

                    using (var targetStream = System.IO.File.Create(UploadFilePath))
                    {
                        await section.Body.CopyToAsync(targetStream);
                    }

                    return Ok();
                }

                section = await reader.ReadNextSectionAsync();
            }

            logger.LogWarning("Upload aborted, no files data in the request.");
            return BadRequest("Es wurde keine Datei hochgeladen.");
        }
    }
}
