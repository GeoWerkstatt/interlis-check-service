using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Serilog;
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
        private readonly IConfiguration configuration;
        private Serilog.ILogger sessionLogger;

        public string SaveToPath { get; set; }
        public UploadController(IHubContext<SignalRHub> hubcontext, ILogger<UploadController> applicationLogger, IConfiguration configuration)
        {
            this.hubContext = hubcontext;
            this.applicationLogger = applicationLogger;
            this.configuration = configuration;
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
            var fileName = request.Query["fileName"][0];

            StartSessionLog(fileName);
            applicationLogger.LogInformation($"Start uploading: {fileName}");
            await hubContext.Clients.Client(connectionId).SendAsync("uploadStarted", $"Upload started for file {fileName}");

            Task<IActionResult> uploadTask = await UploadAndSendUpdatesAsync(request, connectionId);

            if (SaveToPath != null)
            {
                Task<bool> parseTask = await ParseAndSendUpdatesAsync(connectionId);
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

        private async Task<Task<bool>> ParseAndSendUpdatesAsync(string connectionId)
        {
            CancellationTokenSource cts = new ();
            Task<bool> parseTask = IsValidXmlAsync(SaveToPath);
            sessionLogger.Information("Parsing file");
            applicationLogger.LogInformation("Parsing file");
            await Task.WhenAny(parseTask, SendPeriodicUploadFeedback(connectionId, "File is parsing...", cts.Token));
            cts.Cancel();
            cts.Dispose();
            return parseTask;
        }

        private async Task<Task<IActionResult>> UploadAndSendUpdatesAsync(HttpRequest request, string connectionId)
        {
            CancellationTokenSource cts = new ();
            Task<IActionResult> uploadTask = UploadToDirectory(request);
            sessionLogger.Information("Uploading file");
            applicationLogger.LogInformation("Uploading file");
            await Task.WhenAny(uploadTask, SendPeriodicUploadFeedback(connectionId, "File is uploading...", cts.Token));
            cts.Cancel();
            cts.Dispose();
            return uploadTask;
        }

        private async Task<IActionResult> UploadToDirectory(HttpRequest request)
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
                    var uploadPathFormat = configuration.GetSection("Upload")["PathFormat"];
                    var timestamp = DateTime.Now.ToString("yyyy_MM_d_HHmmss");
                    var uploadFileName = $"{timestamp}_{contentDisposition.FileName.Value}";
                    SaveToPath = uploadPathFormat.Replace("{FileName}", uploadFileName);

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
            XmlReaderSettings settings = new ();
            settings.IgnoreWhitespace = true;
            settings.Async = true;

            using (var fileStream = System.IO.File.OpenText(filePath))
            {
                using XmlReader reader = XmlReader.Create(fileStream, settings);
                try
                {
                    while (await reader.ReadAsync())
                    {
                    }
                }
                catch (Exception e)
                {
                    sessionLogger.Information($"Could not parse XTF File: {e.Message}");
                    applicationLogger.LogInformation($"Could not parse XTF File: {e.Message}");
                    return false;
                }
            }

            return true;
        }

        private void StartSessionLog(string uploadedFileName)
        {
            sessionLogger = GetLogger(uploadedFileName);
            sessionLogger.Information($"Start uploading: {uploadedFileName}");
        }

        private Serilog.ILogger GetLogger(string uploadedFileName)
        {
            var sessionPathFormat = configuration.GetSection("Logging")["PathFormatSession"];
            var timestamp = DateTime.Now.ToString("yyyy_MM_d_HHmmss");
            var sessionId = $"{timestamp}_{uploadedFileName}";
            var logFileName = sessionPathFormat.Replace("{SessionId}", sessionId);

            return new LoggerConfiguration()
                .WriteTo.File(logFileName)
                .CreateLogger();
        }
    }
}
