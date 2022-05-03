using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using static ILICheck.Web.Extensions;

namespace ILICheck.Web
{
    public class Validator : IValidator
    {
        private readonly ILogger<Validator> logger;
        private readonly IConfiguration configuration;

        private string gpkgModels;
        private bool isGpkg;
        private bool isZipFile;
        private string uploadFilePath;

        public Validator(ILogger<Validator> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public async Task ValidateAsync(string jobId, bool deleteTransferFile, string uploadFilePath)
        {
            this.uploadFilePath = uploadFilePath;

            isZipFile = Path.GetExtension(uploadFilePath) == ".zip";
            isGpkg = Path.GetExtension(uploadFilePath) == ".gpkg";

            try
            {
                if (isZipFile)
                {
                    await UnzipFileAsync(uploadFilePath);
                }

                if (isGpkg)
                {
                    await ReadGpkgModelNamesAsync(uploadFilePath);
                }
                else
                {
                    // Supported file extensions for additional xml validation
                    var supportedExtensions = new[] { ".xml", ".xtf" };
                    if (supportedExtensions.Contains(Path.GetExtension(uploadFilePath).ToLower()))
                    {
                        await ParseXmlAsync(uploadFilePath);
                    }
                }

                await ValidateAsync(jobId);
            }
            catch (Exception e)
            {
                // TODO: Add log/set state -> validationAborted
                logger.LogError("Unexpected error <{exceptionMessage}>", e.Message);
            }

            if (deleteTransferFile || isGpkg)
            {
                if (!string.IsNullOrEmpty(uploadFilePath))
                {
                    if (isZipFile)
                    {
                        var parentDirectory = Directory.GetParent(uploadFilePath).FullName;

                        // Keep log files with .log and .xtf extension.
                        var filesToDelete = Directory.GetFiles(parentDirectory).Where(file => Path.GetExtension(file).ToLower() != ".log" && Path.GetExtension(file).ToLower() != ".xtf");
                        foreach (var file in filesToDelete)
                        {
                            File.Delete(file);
                        }
                    }

                    logger.LogInformation("Deleting {uploadFilePath}...", uploadFilePath);
                    File.Delete(uploadFilePath);
                }
            }
        }

        private async Task UnzipFileAsync(string zipFilePath)
        {
            // TODO: Move instruction message, maybe split into smaller chunks
            // string uploadInstructionMessage = "Für eine INTERLIS 1 Validierung laden Sie eine .zip-Datei hoch, die eine .itf-Datei und optional eine .ili-Datei mit dem passendem INTERLIS Modell enthält. Für eine INTERLIS 2 Validierung laden Sie eine .xtf-Datei hoch (INTERLIS-Modell wird in öffentlichen Modell-Repositories gesucht). Alternativ laden Sie eine .zip Datei mit einer .xtf-Datei und allen zur Validierung notwendigen INTERLIS-Modellen (.ili) und Katalogdateien (.xml) hoch.";
            await Task.Run(() =>
            {
                logger.LogInformation("Unzipping file");
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

                            uploadFilePath = Directory.GetFiles(parentDirectory).Single(file => Path.GetExtension(file) == transferFileExtension);
                        }
                        else
                        {
                            // TODO: Add log/set state -> validationAborted, Dateipfad konnte nicht aufgelöst werden! {uploadInstructionMessage}
                            logger.LogWarning("Upload aborted, cannot get extraction path.");
                            return;
                        }
                    }

                    System.IO.File.Delete(zipFilePath);
                }
                catch (UnknownExtensionException ex)
                {
                    // TODO: Add log/set state -> validationAborted, Nicht unterstützte Dateien, bitte laden Sie ausschliesslich {string.Join(", ", GetAcceptedFileExtensionsForZipContent())} Dateien hoch! {uploadInstructionMessage}
                    logger.LogInformation(ex.Message);
                }
                catch (TransferFileNotFoundException ex)
                {
                    // TODO: Add log/set state -> validationAborted, Die hochgeladene .zip-Datei enthält keine Transferdatei(en)! {uploadInstructionMessage}
                    logger.LogInformation(ex.Message);
                }
                catch (MultipleTransferFileFoundException ex)
                {
                    // TODO: Add log/set state -> validationAborted, Mehrere Transferdateien gefunden! {uploadInstructionMessage}
                    logger.LogInformation(ex.Message);
                }
                catch (Exception ex)
                {
                    // TODO: Add log/set state -> validationAborted, Unbekannter Fehler
                    logger.LogWarning(ex.Message);
                }
            });
        }

        private async Task ParseXmlAsync(string filePath)
        {
            logger.LogInformation("Parsing file");
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
                logger.LogWarning("Upload aborted, could not parse XTF File: {errorMessage}", e.Message);
            }
        }

        private async Task ReadGpkgModelNamesAsync(string filePath)
        {
            logger.LogInformation("Read model names from GeoPackage");
            try
            {
                var connectionString = $"Data Source={filePath}";
                await Task.Run(() => gpkgModels = ReadGpkgModelNameEntries(connectionString).CleanupGpkgModelNames(configuration));
            }
            catch (Exception e)
            {
                // TODO: Add log/set state -> validationAborted, Fehler beim Auslesen der Modellnamen aus dem GeoPackage
                logger.LogWarning("Upload aborted, could not read model names from the given GeoPackage SQLite database: {errorMessage}", e.Message);
            }
        }

        private async Task ValidateAsync(string connectionId)
        {
            logger.LogInformation("Validating file");
            var uploadPath = configuration.GetSection("Validation")["UploadFolderInContainer"].Replace("{Name}", connectionId);
            var fileName = Path.GetFileName(uploadFilePath);

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
                logger.LogWarning("The ilivalidator found errors in the file. Validation failed.");
            }
            else
            {
                // TODO: Add log/set state -> validatedWithoutErrors, Der ilivalidator hat keine Fehler in der Datei gefunden
                logger.LogInformation("The ilivalidator found no errors in the file. Validation successfull!");
            }

            // TODO: Add log/set state -> stopConnection
            logger.LogInformation("Validation completed: {timestamp}", DateTime.Now);
        }
    }
}
