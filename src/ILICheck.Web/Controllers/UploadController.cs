using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SignalR.Hubs;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ILICheck.Web.Controllers
{
    public class UploadController : Controller
    {
        private readonly IHubContext<SignalRHub> hubContext;
        private readonly ILogger<UploadController> applicationLogger;

        public string SaveToPath { get; set; }
        public UploadController(IHubContext<SignalRHub> hubcontext, ILogger<UploadController> applicationLogger)
        {
            this.hubContext = hubcontext;
            this.applicationLogger = applicationLogger;
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
            var connectionId = request.Query["connectionId"][0];
            CancellationTokenSource uploadCts = new();
            Task<IActionResult> uploadTask = UploadToDirectory(request);
            await Task.WhenAny(uploadTask, SendPeriodicUploadFeedback(connectionId, "File is uploading...", uploadCts.Token));
            uploadCts.Cancel();
            uploadCts.Dispose();

            if (SaveToPath != null)
            {
                CancellationTokenSource parseCts = new();
                Task<bool> parseTask = IsValidXmlAsync(SaveToPath);
                await Task.WhenAny(parseTask, SendPeriodicUploadFeedback(connectionId, "File is parsing...", parseCts.Token));
                parseCts.Cancel();
                parseCts.Dispose();
                if (await parseTask == true)
                {
                    return await uploadTask;
                }
                else
                {
                    return BadRequest("Could not parse XTF file.");
                }
            }
            else
            {
                return BadRequest("Could not get file path.");
            }
        }

        private async Task<IActionResult> UploadToDirectory(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            if (!request.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
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
                    var path = Directory.GetCurrentDirectory();

                    // TODO: read upload path from config
                    var uploadPath = $"{path}\\Upload\\";
                    SaveToPath = Path.Combine(uploadPath, contentDisposition.FileName.Value);

                    using (var targetStream = System.IO.File.Create(SaveToPath))
                    {
                        await section.Body.CopyToAsync(targetStream);
                    }

                    return Ok();
                }

                section = await reader.ReadNextSectionAsync();
            }

            return BadRequest("No files data in the request.");
        }

        private async Task SendPeriodicUploadFeedback(string connectionId, string feedbackMessage, CancellationToken cancellationToken)
        {
            while (true)
            {
                await hubContext.Clients.Client(connectionId).SendAsync("fileUploading", feedbackMessage, cancellationToken: cancellationToken);
                await Task.Delay(20000, cancellationToken);
            }
        }

        private async Task<bool> IsValidXmlAsync(string filePath)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.Async = true;

            using (var fileStream = System.IO.File.OpenText(filePath))
            {
                using (XmlReader reader = XmlReader.Create(fileStream, settings))
                {
                    try
                    {
                        while (await reader.ReadAsync())
                        {
                        }
                    }
                    catch (Exception e)
                    {
                        applicationLogger.LogInformation($"Could not parse XTF File: {e.Message}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
