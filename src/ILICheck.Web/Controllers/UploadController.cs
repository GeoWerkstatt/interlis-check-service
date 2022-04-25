using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using static ILICheck.Web.Extensions;

namespace ILICheck.Web.Controllers
{
    public class UploadController : Controller
    {
        private readonly ILogger<UploadController> applicationLogger;
        private readonly IConfiguration configuration;
        private Serilog.ILogger sessionLogger;
        private string gpkgModels;
        private bool isGpkg;
        private bool isZipFile;

        public string UploadFolderPath { get; set; }
        public string UploadFilePath { get; set; }

        public UploadController(ILogger<UploadController> applicationLogger, IConfiguration configuration)
        {
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
            var connectionId = Guid.NewGuid().ToString();
            var fileName = request.Query["fileName"][0];
            var deleteTransferFile = string.Equals(
                Environment.GetEnvironmentVariable("DELETE_TRANSFER_FILES", EnvironmentVariableTarget.Process),
                "true",
                StringComparison.OrdinalIgnoreCase);

            MakeUploadFolder(connectionId);

            sessionLogger = GetLogger(fileName);
            LogInfo($"Start uploading: {fileName}");
            LogInfo($"File size: {request.ContentLength}");
            LogInfo($"Start time: {DateTime.Now}");
            LogInfo($"Delete transfer file after validation: {deleteTransferFile}");

            // TODO: Handle exception while uploading file...
            await UploadToDirectoryAsync(request);

            _ = DoValidationAsync(connectionId, deleteTransferFile);

            LogInfo($"Successfully received file: {DateTime.Now}");

            // TODO: Return upload result, default HTTP 201
            return new JsonResult(new
            {
                jobId = connectionId,
                statusUrl = "api/v1/status/ff11cb95-1a91-4fea-ba86-2ed5c35a0d56",
            });
        }

        private async Task DoValidationAsync(string connectionId, bool deleteTransferFile)
        {
            try
            {
                if (isZipFile)
                {
                    await UnzipFileAsync(UploadFilePath);
                }

                if (isGpkg)
                {
                    await ReadGpkgModelNamesAsync(UploadFilePath);
                }
                else
                {
                    // Supported file extensions for additional xml validation
                    var supportedExtensions = new[] { ".xml", ".xtf" };
                    if (supportedExtensions.Contains(Path.GetExtension(UploadFilePath).ToLower()))
                    {
                        await ParseXmlAsync(UploadFilePath, connectionId);
                    }
                }

                await ValidateAsync(connectionId);
            }
            catch (Exception e)
            {
                // TODO: Add log/set state -> validationAborted
                LogInfo($"Unexpected error: {e.Message}");
            }

            if (deleteTransferFile || isGpkg)
            {
                if (!string.IsNullOrEmpty(UploadFilePath))
                {
                    if (isZipFile)
                    {
                        var parentDirectory = Directory.GetParent(UploadFilePath).FullName;

                        // Keep log files with .log and .xtf extension.
                        var filesToDelete = Directory.GetFiles(parentDirectory).Where(file => Path.GetExtension(file).ToLower() != ".log" && Path.GetExtension(file).ToLower() != ".xtf");
                        foreach (var file in filesToDelete)
                        {
                            System.IO.File.Delete(file);
                        }
                    }

                    LogInfo($"Deleting {UploadFilePath}...");
                    System.IO.File.Delete(UploadFilePath);
                }
            }
        }

        private void MakeUploadFolder(string connectionId)
        {
            UploadFolderPath = configuration.GetUploadPathForSession(connectionId);
            Directory.CreateDirectory(UploadFolderPath);
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

                    isZipFile = Path.GetExtension(UploadFilePath) == ".zip";
                    isGpkg = Path.GetExtension(UploadFilePath) == ".gpkg";

                    return Ok();
                }

                section = await reader.ReadNextSectionAsync();
            }

            LogInfo("Upload aborted, no files data in the request.");
            return BadRequest("Es wurde keine Datei hochgeladen.");
        }

        private async Task UnzipFileAsync(string zipFilePath)
        {
            // TODO: Move instruction message, maybe split into smaller chunks
            // string uploadInstructionMessage = "Für eine INTERLIS 1 Validierung laden Sie eine .zip-Datei hoch, die eine .itf-Datei und optional eine .ili-Datei mit dem passendem INTERLIS Modell enthält. Für eine INTERLIS 2 Validierung laden Sie eine .xtf-Datei hoch (INTERLIS-Modell wird in öffentlichen Modell-Repositories gesucht). Alternativ laden Sie eine .zip Datei mit einer .xtf-Datei und allen zur Validierung notwendigen INTERLIS-Modellen (.ili) und Katalogdateien (.xml) hoch.";
            await Task.Run(() =>
            {
                LogInfo("Unzipping file");
                var uploadPath = Path.GetFullPath(configuration.GetSection("Upload")["PathFormat"]);
                var extractPath = Path.GetDirectoryName(uploadPath);

                // Ensures that the last character on the extraction path is the directory separator char.
                // Without this, a malicious zip file could try to traverse outside of the expected extraction path.
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    extractPath += Path.DirectorySeparatorChar;
                }

                try
                {
                    using (var archive = ZipFile.OpenRead(zipFilePath))
                    {
                        var transferFileExtension = archive.Entries
                            .Select(entry => Path.GetExtension(entry.FullName))
                            .GetTransferFileExtension();

                        if (Path.GetFullPath(zipFilePath).StartsWith(extractPath, StringComparison.Ordinal))
                        {
                            var parentDirectory = Directory.GetParent(zipFilePath).FullName;
                            foreach (var entry in archive.Entries)
                            {
                                entry.ExtractToFile(Path.Combine(parentDirectory, entry.Name));
                            }

                            UploadFilePath = Directory.GetFiles(parentDirectory).Single(file => Path.GetExtension(file) == transferFileExtension);
                        }
                        else
                        {
                            // TODO: Add log/set state -> validationAborted, Dateipfad konnte nicht aufgelöst werden! {uploadInstructionMessage}
                            LogInfo("Upload aborted, cannot get extraction path.");
                            return;
                        }
                    }

                    System.IO.File.Delete(zipFilePath);
                }
                catch (UnknownExtensionException ex)
                {
                    // TODO: Add log/set state -> validationAborted, Nicht unterstützte Dateien, bitte laden Sie ausschliesslich {string.Join(", ", GetAcceptedFileExtensionsForZipContent())} Dateien hoch! {uploadInstructionMessage}
                    LogInfo(ex.Message);
                }
                catch (TransferFileNotFoundException ex)
                {
                    // TODO: Add log/set state -> validationAborted, Die hochgeladene .zip-Datei enthält keine Transferdatei(en)! {uploadInstructionMessage}
                    LogInfo(ex.Message);
                }
                catch (MultipleTransferFileFoundException ex)
                {
                    // TODO: Add log/set state -> validationAborted, Mehrere Transferdateien gefunden! {uploadInstructionMessage}
                    LogInfo(ex.Message);
                }
                catch (Exception ex)
                {
                    // TODO: Add log/set state -> validationAborted, Unbekannter Fehler
                    LogInfo(ex.Message);
                }
            });
        }

        private async Task ParseXmlAsync(string filePath, string connectionId)
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
                // TODO: Add log/set state -> validationAborted, Datei hat keine gültige XML-Struktur
                LogInfo($"Upload aborted, could not parse XTF File: {e.Message}");
            }
        }

        private async Task ReadGpkgModelNamesAsync(string filePath)
        {
            LogInfo("Read model names from GeoPackage");
            try
            {
                var connectionString = $"Data Source={filePath}";
                await Task.Run(() => gpkgModels = ReadGpkgModelNameEntries(connectionString).CleanupGpkgModelNames(configuration));
            }
            catch (Exception e)
            {
                // TODO: Add log/set state -> validationAborted, Fehler beim Auslesen der Modellnamen aus dem GeoPackage
                LogInfo($"Upload aborted, could not read model names from the given GeoPackage SQLite database: {e.Message}");
            }
        }

        private async Task ValidateAsync(string connectionId)
        {
            LogInfo("Validating file");
            var uploadPath = configuration.GetSection("Validation")["UploadFolderInContainer"].Replace("{Name}", connectionId);
            var fileName = Path.GetFileName(UploadFilePath);

            var filePath = uploadPath + $"/{fileName}";
            var logPath = uploadPath + "/ilivalidator_output.log";
            var xtfLogPath = uploadPath + "/ilivalidator_output.xtf";

            var commandPrefix = configuration.GetSection("Validation")["CommandPrefix"];
            var options = $"--log {logPath} --xtflog {xtfLogPath}";
            if (isGpkg) options = $"{options} --models \"{gpkgModels}\"";
            var command = $"{commandPrefix}ilivalidator {options} \"{filePath}\"";

            var startInfo = new ProcessStartInfo()
            {
                FileName = configuration.GetShellExecutable(),
                Arguments = command,
                UseShellExecute = true,
            };

            using var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                // TODO: Add log/set state -> validatedWithErrors, Der ilivalidator hat Fehler in der Datei gefunden
                LogInfo("The ilivalidator found errors in the file. Validation failed.");
            }
            else
            {
                // TODO: Add log/set state -> validatedWithoutErrors, Der ilivalidator hat keine Fehler in der Datei gefunden
                LogInfo("The ilivalidator found no errors in the file. Validation successfull!");
            }

            // TODO: Add log/set state -> stopConnection
            LogInfo($"Validation completed: {DateTime.Now}");
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
