﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Serilog;
using SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public IActionResult UploadResult { get; set; }

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

            sessionLogger = GetLogger(fileName);
            LogInfo($"Start uploading: {fileName}");
            await hubContext.Clients.Client(connectionId).SendAsync("uploadStarted", $"Upload started for file {fileName}");

            using var cts = new CancellationTokenSource();
            try
            {
                await Task.Run(() => DoTaskWhileSendingUpdates(UploadToDirectoryAsync(request, cts), connectionId, "File is uploading..."), cts.Token);
                if (Path.GetExtension(SaveToPath) == ".zip")
                {
                    await Task.Run(() => DoTaskWhileSendingUpdates(UnzipFileAsync(SaveToPath, cts), connectionId, "File is being unzipped..."), cts.Token);
                }

                await Task.Run(() => DoTaskWhileSendingUpdates(ParseXmlAsync(SaveToPath, cts), connectionId, "File is parsinig..."), cts.Token);
                await Task.Run(() => DoTaskWhileSendingUpdates(ValidateFileAsync(cts), connectionId, "File is being validated..."), cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown\n");
            }

            if (UploadResult.GetType() != typeof(OkResult))
            {
                System.IO.File.Delete(SaveToPath);
            }

            return UploadResult;
        }

        private async Task DoTaskWhileSendingUpdates(Task task, string connectionId, string updateMessage)
        {
            using var cts = new CancellationTokenSource();
            await Task.WhenAny(task, SendPeriodicUploadFeedback(connectionId, updateMessage, cts.Token));
            cts.Cancel();
            return;
        }

        private async Task SendPeriodicUploadFeedback(string connectionId, string feedbackMessage, CancellationToken cancellationToken)
        {
            while (true)
            {
                await hubContext.Clients.Client(connectionId).SendAsync("fileUploading", feedbackMessage, cancellationToken: cancellationToken);
                await Task.Delay(2000, cancellationToken);
            }
        }

        private async Task UploadToDirectoryAsync(HttpRequest request, CancellationTokenSource mainCts)
        {
            LogInfo("Uploading file");
            if (!request.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                UploadResult = new UnsupportedMediaTypeResult();
                LogInfo("Upload aborted, unsupported media type.");
                mainCts.Cancel();
                return;
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

                    UploadResult = Ok();
                    return;
                }

                section = await reader.ReadNextSectionAsync();
            }

            UploadResult = BadRequest("No files data in the request.");
            LogInfo("Upload aborted, no files data in the request.");
            mainCts.Cancel();
            return;
        }

        private async Task UnzipFileAsync(string zipFilePath, CancellationTokenSource mainCts)
        {
            await Task.Run(() =>
            {
                LogInfo("Unzipping file");
                string extractPath = @".\Upload";
                extractPath = Path.GetFullPath(extractPath);

                // Ensures that the last character on the extraction path
                // is the directory separator char.
                // Without this, a malicious zip file could try to traverse outside of the expected
                // extraction path.
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    extractPath += Path.DirectorySeparatorChar;
                }

                string unzippedFilePath = "";
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    if (archive.Entries.Count != 1)
                    {
                        UploadResult = BadRequest("Only zip archives containing exactly one file are supported.");
                        LogInfo("Upload aborted, only zip archives containing exactly one file are supported.");
                        mainCts.Cancel();
                        return;
                    }
                    else
                    {
                        var extention = Path.GetExtension(archive.Entries[0].FullName);
                        var supportedExtension = new List<string>() { ".xtf", ".xml" };
                        if (supportedExtension.Contains(extention))
                        {
                            unzippedFilePath = Path.GetFullPath(Path.ChangeExtension(zipFilePath, Path.GetExtension(archive.Entries[0].FullName)));
                            if (unzippedFilePath.StartsWith(extractPath, StringComparison.Ordinal))
                            {
                                // Overwrite file if it exists.
                                archive.Entries[0].ExtractToFile(unzippedFilePath, true);
                            }
                            else
                            {
                                UploadResult = BadRequest("Cannot get extraction path.");
                                LogInfo("Upload aborted, cannot get extraction path.");
                                mainCts.Cancel();
                                return;
                            }
                        }
                        else
                        {
                            UploadResult = BadRequest("Zipped file has unsupported extension.");
                            LogInfo("Upload aborted, zipped file has unsupported extension.");
                            mainCts.Cancel();
                            return;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(unzippedFilePath))
                {
                    System.IO.File.Delete(zipFilePath);
                    SaveToPath = unzippedFilePath;
                    return;
                }
                else
                {
                    UploadResult = BadRequest("Unknown error.");
                    LogInfo("Upload aborted, unknown error.");
                    mainCts.Cancel();
                    return;
                }
            });
        }

        private async Task ParseXmlAsync(string filePath, CancellationTokenSource mainCts)
        {
            LogInfo("Parsing file");
            var settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.Async = true;

            try
            {
                using var fileStream = System.IO.File.OpenText(filePath);
                using XmlReader reader = XmlReader.Create(fileStream, settings);
                while (await reader.ReadAsync())
                {
                }
            }
            catch (Exception e)
            {
                UploadResult = BadRequest("Could not parse XTF File");
                LogInfo($"Upload aborted, could not parse XTF File: {e.Message}");
                mainCts.Cancel();
                return;
            }

            return;
        }

        private async Task ValidateFileAsync(CancellationTokenSource cts)
        {
            await Task.Delay(5000, cts.Token);
            return;
        }

        private void LogInfo(string logMessage)
        {
            sessionLogger.Information(logMessage);
            applicationLogger.LogInformation(logMessage);
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
