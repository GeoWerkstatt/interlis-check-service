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

        public string UploadFolderPath { get; set; }
        public string UploadFilePath { get; set; }

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

            MakeUploadFolder(connectionId);

            sessionLogger = GetLogger(fileName);
            LogInfo($"Start uploading: {fileName}");
            await hubContext.Clients.Client(connectionId).SendAsync("uploadStarted", $"Upload von {fileName} gestartet.");

            using var internalTokenSource = new CancellationTokenSource();
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(internalTokenSource.Token, HttpContext.RequestAborted))
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        var taskToExecute = UploadToDirectoryAsync(request, cts);
                        await DoTaskWhileSendingUpdatesAsync(taskToExecute, connectionId, "Datei wird hochgeladen...");
                        if (taskToExecute.IsFaulted) throw taskToExecute.Exception;
                    }, cts.Token);

                    if (Path.GetExtension(UploadFilePath) == ".zip")
                    {
                        await Task.Run(async () =>
                        {
                            var taskToExecute1 = UnzipFileAsync(UploadFilePath, cts);
                            await DoTaskWhileSendingUpdatesAsync(taskToExecute1, connectionId, "Datei wird entzipped...");
                            if (taskToExecute1.IsFaulted) throw taskToExecute1.Exception;
                        }, cts.Token);
                    }

                    await Task.Run(async () =>
                    {
                        var taskToExecute2 = ParseXmlAsync(UploadFilePath, cts);
                        await DoTaskWhileSendingUpdatesAsync(taskToExecute2, connectionId, "Dateistruktur validieren...");
                        if (taskToExecute2.IsFaulted) throw taskToExecute2.Exception;
                    }, cts.Token);

                    await Task.Run(async () =>
                    {
                        var taskToExecute3 = ValidateFileAsync(fileName, cts);
                        await DoTaskWhileSendingUpdatesAsync(taskToExecute3, connectionId, "Datei validated...");
                        if (taskToExecute3.IsFaulted) throw taskToExecute3.Exception;
                    }, cts.Token);
                }
                catch (Exception e)
                {
                    LogInfo($"Unexpected error: {e.Message}");
                    if (UploadResult == null)
                    {
                        UploadResult = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }
                }
            }

            if (UploadResult.GetType() != typeof(OkResult))
            {
                System.IO.File.Delete(UploadFilePath);
            }

            // Close connection after file upload attempt, to make a new connection for next file.
            await hubContext.Clients.Client(connectionId).SendAsync("stopConnection");
            return UploadResult;
        }

        private void MakeUploadFolder(string connectionId)
        {
            var uploadPathFormat = configuration.GetSection("Upload")["PathFormat"];
            var folderName = connectionId;
            UploadFolderPath = uploadPathFormat.Replace("{Name}", folderName);
            Directory.CreateDirectory(UploadFolderPath);
        }

        private async Task DoTaskWhileSendingUpdatesAsync(Task task, string connectionId, string updateMessage)
        {
            using var cts = new CancellationTokenSource();
            await Task.WhenAny(task, SendPeriodicUploadFeedbackAsync(connectionId, updateMessage, cts.Token));
            cts.Cancel();
            return;
        }

        private async Task SendPeriodicUploadFeedbackAsync(string connectionId, string feedbackMessage, CancellationToken cancellationToken)
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
                    var timestamp = DateTime.Now.ToString("yyyy_MM_d_HHmmss");
                    var uploadFileName = $"{timestamp}_{contentDisposition.FileName.Value}";
                    UploadFilePath = Path.Combine(UploadFolderPath, uploadFileName);

                    using (var targetStream = System.IO.File.Create(UploadFilePath))
                    {
                        await section.Body.CopyToAsync(targetStream);
                    }

                    UploadResult = Ok();
                    return;
                }

                section = await reader.ReadNextSectionAsync();
            }

            UploadResult = BadRequest("Es wurde keine Datei hochgeladen.");
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
                using (var archive = ZipFile.OpenRead(zipFilePath))
                {
                    if (archive.Entries.Count != 1)
                    {
                        UploadResult = BadRequest("Nur Zip-Archive die genau ein file enthalten, werden unterstützt.");
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
                                archive.Entries[0].ExtractToFile(unzippedFilePath);
                            }
                            else
                            {
                                UploadResult = BadRequest("Dateipfad konnte nicht aufgelöst werden.");
                                LogInfo("Upload aborted, cannot get extraction path.");
                                mainCts.Cancel();
                                return;
                            }
                        }
                        else
                        {
                            UploadResult = BadRequest("Nicht unterstützte Dateiendung.");
                            LogInfo("Upload aborted, zipped file has unsupported extension.");
                            mainCts.Cancel();
                            return;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(unzippedFilePath))
                {
                    System.IO.File.Delete(zipFilePath);
                    UploadFilePath = unzippedFilePath;
                    return;
                }
                else
                {
                    UploadResult = BadRequest("Unbekannter Fehler.");
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

            using var fileStream = System.IO.File.OpenText(filePath);
            using var reader = XmlReader.Create(fileStream, settings);
            try
            {
                while (await reader.ReadAsync())
                {
                }
            }
            catch (XmlException e)
            {
                UploadResult = BadRequest("Datei hat keine gültige XML-Struktur.");
                LogInfo($"Upload aborted, could not parse XTF File: {e.Message}");
                mainCts.Cancel();
                return;
            }

            return;
        }

        private async Task ValidateFileAsync(string fileName, CancellationTokenSource cts)
        {
            MakeMockIlivalidatorLog(fileName);
            await Task.Delay(5000, cts.Token);
            return;
        }

        private void MakeMockIlivalidatorLog(string fileName)
        {
            var ilivalidatorLog = $"Ilivalidator_{fileName}.xtf";
            var logFilePath = Path.Combine(UploadFolderPath, ilivalidatorLog);
            using (var outputFile = new StreamWriter(logFilePath))
            {
                outputFile.WriteLine("The Ilivalidator output: ... ");
            }
        }

        private void LogInfo(string logMessage)
        {
            sessionLogger.Information(logMessage);
            applicationLogger.LogInformation(logMessage);
        }

        private Serilog.ILogger GetLogger(string uploadedFileName)
        {
            var timestamp = DateTime.Now.ToString("yyyy_MM_d_HHmmss");
            var logFileName = $"Session_{timestamp}_{uploadedFileName}.log";
            var logFilePath = Path.Combine(UploadFolderPath, logFileName);

            return new LoggerConfiguration()
                .WriteTo.File(logFilePath)
                .CreateLogger();
        }
    }
}
