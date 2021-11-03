using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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

namespace ILICheck.Web.Controllers
{
    public class UploadController : Controller
    {
        private readonly IHubContext<SignalRHub> hubContext;
        private readonly ILogger<UploadController> applicationLogger;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;
        private Serilog.ILogger sessionLogger;

        public CancellationTokenSource UploadCts { get; set; }
        public string CurrentConnectionId { get; set; }

        public string UploadFolderPath { get; set; }
        public string UploadFilePath { get; set; }

        public UploadController(IHubContext<SignalRHub> hubContext, ILogger<UploadController> applicationLogger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.hubContext = hubContext;
            this.applicationLogger = applicationLogger;
            this.configuration = configuration;
            this.environment = environment;
            SignalRConnectionHelper.Disconnected += ValidationAborted;
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
            CurrentConnectionId = connectionId;
            UploadCts = new CancellationTokenSource();
            var fileName = request.Query["fileName"][0];
            var deleteXtfTransferFile = string.Equals(
                Environment.GetEnvironmentVariable("DELETE_TRANSFER_FILES", EnvironmentVariableTarget.Process),
                "true",
                StringComparison.InvariantCultureIgnoreCase);

            MakeUploadFolder(connectionId);

            sessionLogger = GetLogger(fileName);
            LogInfo($"Start uploading: {fileName}");
            LogInfo($"File size: {request.ContentLength}");
            LogInfo($"Start time: {DateTime.Now}");
            LogInfo($"Delete XTF transfer file after validation: {deleteXtfTransferFile}");

            var uploadTask = UploadToDirectoryAsync(request);
            await DoTaskWhileSendingUpdatesAsync(uploadTask, connectionId, null);
            if (uploadTask.IsFaulted) throw uploadTask.Exception;

            DoValidation(connectionId, deleteXtfTransferFile, UploadCts);

            LogInfo($"Stop time: {DateTime.Now}");
            return uploadTask.Result;
        }

        private async void DoValidation(string connectionId, bool deleteXtfTransferFile, CancellationTokenSource uploadCts)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(uploadCts.Token, HttpContext.RequestAborted))
            {
                try
                {
                    if (Path.GetExtension(UploadFilePath) == ".zip")
                    {
                        await Task.Run(async () =>
                        {
                            var unzipTask = UnzipFileAsync(UploadFilePath, cts, connectionId);
                            await DoTaskWhileSendingUpdatesAsync(unzipTask, connectionId, "Datei wird entzipped...");
                            if (unzipTask.IsFaulted) throw unzipTask.Exception;
                        }, cts.Token);
                    }

                    await Task.Run(async () =>
                    {
                        var parseTask = ParseXmlAsync(UploadFilePath, cts, connectionId);
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
                    await hubContext.Clients.Client(connectionId).SendAsync("validationAborted");
                }

                if (deleteXtfTransferFile || cts.Token.IsCancellationRequested)
                {
                    if (!string.IsNullOrEmpty(UploadFilePath))
                    {
                        LogInfo($"Deleting {UploadFilePath}...");
                        System.IO.File.Delete(UploadFilePath);
                    }
                }

                await hubContext.Clients.Client(connectionId).SendAsync("validationOk");
            }
        }

        private void MakeUploadFolder(string connectionId)
        {
            UploadFolderPath = configuration.GetUploadPathForSession(connectionId);
            Directory.CreateDirectory(UploadFolderPath);
        }

        private async Task DoTaskWhileSendingUpdatesAsync(Task taskToRun, string connectionId, string updateMessage)
        {
            using var cts = new CancellationTokenSource();

            await Task.WhenAny(
                taskToRun,
                Task.Run(async () =>
                {
                    // Periodically update client log
                    while (true)
                    {
                        await UpdateClientLogAsync(connectionId, updateMessage, cts.Token);
                        await Task.Delay(2000, cts.Token);
                    }
                }));

            cts.Cancel();
            return;
        }

        private async Task UpdateClientLogAsync(string connectionId, string message, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message))
            {
                await hubContext.Clients.Client(connectionId).SendAsync("updateLog", message, cancellationToken);
            }
        }

        private async Task<IActionResult> UploadToDirectoryAsync(HttpRequest request)
        {
            LogInfo("Uploading file");
            if (!request.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                LogInfo("Upload aborted, unsupported media type.");
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

            LogInfo("Upload aborted, no files data in the request.");
            return BadRequest("Es wurde keine Datei hochgeladen.");
        }

        private async Task UnzipFileAsync(string zipFilePath, CancellationTokenSource mainCts, string connectionId)
        {
            await Task.Run(async () =>
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
                        await hubContext.Clients.Client(connectionId).SendAsync("validationAborted", "Nur Zip-Archive, die genau eine Datei enthalten, werden unterstützt.");

                        LogInfo("Upload aborted, only zip archives containing exactly one file are supported.");
                        mainCts.Cancel();
                        return;
                    }
                    else
                    {
                        var extention = Path.GetExtension(archive.Entries[0].FullName);
                        var supportedExtension = new List<string>() { ".xtf", ".xml" };
                        if (supportedExtension.Contains(extention.ToLower()))
                        {
                            unzippedFilePath = Path.GetFullPath(Path.ChangeExtension(zipFilePath, Path.GetExtension(archive.Entries[0].FullName)));
                            if (unzippedFilePath.StartsWith(extractPath, StringComparison.Ordinal))
                            {
                                archive.Entries[0].ExtractToFile(unzippedFilePath);
                            }
                            else
                            {
                                await hubContext.Clients.Client(connectionId).SendAsync("validationAborted", "Dateipfad konnte nicht aufgelöst werden.");
                                LogInfo("Upload aborted, cannot get extraction path.");
                                mainCts.Cancel();
                                return;
                            }
                        }
                        else
                        {
                            await hubContext.Clients.Client(connectionId).SendAsync("validationAborted", "Nicht unterstützte Dateiendung.");
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
                    await hubContext.Clients.Client(connectionId).SendAsync("validationAborted", "Unbekannter Fehler.");
                    LogInfo("Upload aborted, unknown error.");
                    mainCts.Cancel();
                    return;
                }
            });
        }

        private async Task ParseXmlAsync(string filePath, CancellationTokenSource mainCts, string connectionId)
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
                await hubContext.Clients.Client(connectionId).SendAsync("validationAborted", "Datei hat keine gültige XML-Struktur.");
                LogInfo($"Upload aborted, could not parse XTF File: {e.Message}");
                mainCts.Cancel();
                return;
            }

            return;
        }

        private async Task ValidateFileAsync(string connectionId)
        {
            await Task.Run(async () =>
            {
                LogInfo("Validating file");
                await ValidateAsync(connectionId);
                return;
            });
        }

        private async Task ValidateAsync(string connectionId)
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
                LogInfo("The ilivalidator found errors in the file. Validation failed.");
                await hubContext.Clients.Client(connectionId).SendAsync("validatedWithErrors", "Der Ilivalidator hat Fehler in der Datei gefunden.");
            }
            else
            {
                LogInfo("The ilivalidator found no errors in the file. Validation successfull!");
                await hubContext.Clients.Client(connectionId).SendAsync("validatedWithoutErrors", "Der Ilivalidator hat keine Fehler in der Datei gefunden.");
            }

            await hubContext.Clients.Client(connectionId).SendAsync("stopConnection");
        }

        private void SignalRConnectionDisconnected(object sender, SignalRDisconnectedEventArgs args)
        {
            if (args.ConnectionId == CurrentConnectionId)
            {
                UploadCts.Cancel();
                Console.WriteLine("Validation aborted {1}.", sender);
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
