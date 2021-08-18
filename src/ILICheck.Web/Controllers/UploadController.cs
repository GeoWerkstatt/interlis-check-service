using Microsoft.AspNetCore.Hosting;
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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static ILICheck.Web.Extensions;

namespace ILICheck.Web.Controllers
{
    public class UploadController : Controller
    {
        private readonly IHubContext<SignalRHub> hubContext;
        private readonly ILogger<UploadController> applicationLogger;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;
        private Serilog.ILogger sessionLogger;

        public string UploadFolderPath { get; set; }
        public string UploadFilePath { get; set; }

        public IActionResult ValidationResult { get; set; }

        public UploadController(IHubContext<SignalRHub> hubcontext, ILogger<UploadController> applicationLogger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.hubContext = hubcontext;
            this.applicationLogger = applicationLogger;
            this.configuration = configuration;
            this.environment = environment;
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
            LogInfo($"File size: {request.ContentLength}");
            LogInfo($"Start time: {DateTime.Now}");
            await hubContext.Clients.Client(connectionId).SendAsync("uploadStarted", $"{fileName} wird hochgeladen.");

            using var internalTokenSource = new CancellationTokenSource();
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(internalTokenSource.Token, HttpContext.RequestAborted))
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        var uploadTask = UploadToDirectoryAsync(request, cts);
                        await DoTaskWhileSendingUpdatesAsync(uploadTask, connectionId, "Datei wird hochgeladen...");
                        if (uploadTask.IsFaulted) throw uploadTask.Exception;
                    }, cts.Token);

                    if (Path.GetExtension(UploadFilePath) == ".zip")
                    {
                        await Task.Run(async () =>
                        {
                            var unzipTask = UnzipFileAsync(UploadFilePath, cts);
                            await DoTaskWhileSendingUpdatesAsync(unzipTask, connectionId, "Datei wird entzipped...");
                            if (unzipTask.IsFaulted) throw unzipTask.Exception;
                        }, cts.Token);
                    }

                    await Task.Run(async () =>
                    {
                        var parseTask = ParseXmlAsync(UploadFilePath, cts);
                        await DoTaskWhileSendingUpdatesAsync(parseTask, connectionId, "Dateistruktur validieren...");
                        if (parseTask.IsFaulted) throw parseTask.Exception;
                    }, cts.Token);

                    await Task.Run(async () =>
                    {
                        var validateTask = ValidateFileAsync(connectionId);
                        await DoTaskWhileSendingUpdatesAsync(validateTask, connectionId, "Datei validieren...");
                        if (validateTask.IsFaulted) throw validateTask.Exception;
                    }, cts.Token);
                }
                catch (Exception e)
                {
                    LogInfo($"Unexpected error: {e.Message}");
                    if (ValidationResult == null)
                    {
                        ValidationResult = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }
                }
            }

            if (ValidationResult.GetType() != typeof(OkResult))
            {
                System.IO.File.Delete(UploadFilePath);
            }

            // Close connection after file upload attempt, to make a new connection for next file.
            LogInfo($"Stop time: {DateTime.Now}");
            await hubContext.Clients.Client(connectionId).SendAsync("stopConnection");
            return ValidationResult;
        }

        private void MakeUploadFolder(string connectionId)
        {
            UploadFolderPath = configuration.GetUploadPathForSession(connectionId);
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
                ValidationResult = new UnsupportedMediaTypeResult();
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

                    return;
                }

                section = await reader.ReadNextSectionAsync();
            }

            ValidationResult = BadRequest("Es wurde keine Datei hochgeladen.");
            LogInfo("Upload aborted, no files data in the request.");
            mainCts.Cancel();
            return;
        }

        private async Task UnzipFileAsync(string zipFilePath, CancellationTokenSource mainCts)
        {
            await Task.Run(() =>
            {
                LogInfo("Unzipping file");
                var uploadPath = Path.GetFullPath(configuration.GetSection("Upload")["PathFormat"]);
                var extractPath = Path.GetDirectoryName(uploadPath);

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
                        ValidationResult = BadRequest("Nur Zip-Archive, die genau eine Datei enthalten, werden unterstützt.");
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
                                ValidationResult = BadRequest("Dateipfad konnte nicht aufgelöst werden.");
                                LogInfo("Upload aborted, cannot get extraction path.");
                                mainCts.Cancel();
                                return;
                            }
                        }
                        else
                        {
                            ValidationResult = BadRequest("Nicht unterstützte Dateiendung.");
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
                    ValidationResult = BadRequest("Unbekannter Fehler.");
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
                ValidationResult = BadRequest("Datei hat keine gültige XML-Struktur.");
                LogInfo($"Upload aborted, could not parse XTF File: {e.Message}");
                mainCts.Cancel();
                return;
            }

            return;
        }

        private async Task ValidateFileAsync(string connectionId)
        {
            await Task.Run(() =>
            {
                LogInfo("Validating file");
                Validate(connectionId);
                return;
            });
        }

        private void Validate(string connectionId)
        {
            var uploadPath = configuration.GetSection("Validation")["UploadFolderInContainer"].Replace("{Name}", connectionId);
            var fileName = Path.GetFileName(UploadFilePath);

            var filePath = uploadPath + $"/{fileName}";
            var logPath = uploadPath + "/ilivalidator_output.log";
            var xtfLogPath = uploadPath + "/ilivalidator_output.xtf";

            var commandPrefix = configuration.GetSection("Validation")["CommandPrefix"];
            var command = $"ilivalidator --log {logPath} --xtflog {xtfLogPath} {filePath}";

            var startInfo = new ProcessStartInfo()
            {
                FileName = configuration.GetShellExecutable(),
                Arguments = $"{commandPrefix}{command}",
                UseShellExecute = true,
            };

            using var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                ValidationResult = Ok("Der Ilivalidator hat Fehler in der Datei gefunden.");
            }
            else
            {
                ValidationResult = Ok();
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
