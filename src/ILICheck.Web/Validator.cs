﻿using Microsoft.Extensions.Configuration;
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
    /// <summary>
    /// Validates an INTERLIS <see cref="TransferFile"/> at the given <see cref="HomeDirectory"/>.
    /// </summary>
    public class Validator : IValidator
    {
        private readonly ILogger<Validator> logger;
        private readonly IConfiguration configuration;
        private readonly IFileProvider fileProvider;

        /// <inheritdoc/>
        public virtual string Id { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public virtual string HomeDirectory => fileProvider.HomeDirectory.FullName;

        /// <inheritdoc/>
        public virtual string TransferFile { get; private set; }

        /// <inheritdoc/>
        public virtual string GpkgModelNames { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        public Validator(ILogger<Validator> logger, IConfiguration configuration, IFileProvider fileProvider)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.fileProvider = fileProvider;

            this.fileProvider.Initialize(Id);
        }

        /// <inheritdoc/>
        public async Task ValidateAsync(string transferFile)
        {
            if (transferFile == null) throw new ArgumentNullException(nameof(transferFile));
            if (string.IsNullOrWhiteSpace(transferFile)) throw new ArgumentException("Transfer file name cannot be empty.", nameof(transferFile));
            if (!File.Exists(transferFile)) throw new InvalidOperationException(string.Format("Transfer file with the specified name <{0}> not found in <{1}>.", transferFile, fileProvider.HomeDirectory));

            // Set the fully qualified path to the transfer file.
            TransferFile = transferFile;

            try
            {
                // Unzip compressed file
                if (Path.GetExtension(TransferFile) == ".zip")
                {
                    await UnzipCompressedFileAsync();
                }

                // Read model names from GeoPackage
                if (Path.GetExtension(TransferFile) == ".gpkg")
                {
                    GpkgModelNames = await ReadGpkgModelNamesAsync();
                }

                // Additional xml validation for supported files
                var supportedExtensions = new[] { ".xml", ".xtf" };
                if (supportedExtensions.Contains(Path.GetExtension(TransferFile), StringComparer.OrdinalIgnoreCase))
                {
                    await ValidateXmlAsync();
                }

                // Execute validation with ilivalidator
                await ValidateAsync();

                // Clean up user uploaded/uncompressed files
                await CleanUploadDirectoryAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Unexpected error <{ErrorMessage}>", ex.Message);
            }
        }

        /// <summary>
        /// Asynchronously unzips the content of the compressed <see cref="TransferFile"/> into the same directory.
        /// If succeeded, the original compressed transfer file gets deleted and <see cref="TransferFile"/> path is set to the new file.
        /// </summary>
        /// <exception cref="NotSupportedException">If <see cref="TransferFile"/> file extension is not supported.</exception>
        internal async Task UnzipCompressedFileAsync()
        {
            if (!string.Equals(Path.GetExtension(TransferFile), ".zip", StringComparison.OrdinalIgnoreCase)) throw new NotSupportedException("Only .zip files are supported.");

            logger.LogInformation("Unzipping compressed file <{TransferFile}>", TransferFile);

            await Task.Run(() =>
            {
                try
                {
                    using var archive = ZipFile.OpenRead(Path.Combine(fileProvider.HomeDirectory.FullName, TransferFile));
                    var fileExtensionsInZipArchive = archive.Entries.Select(entry => Path.GetExtension(entry.FullName));
                    var transferFileExtension = configuration.GetTransferFileExtension(fileExtensionsInZipArchive);

                    foreach (var entry in archive.Entries)
                    {
                        entry.ExtractToFile(Path.Combine(fileProvider.HomeDirectory.FullName, entry.Name));
                    }

                    // Set new transfer file
                    TransferFile = fileProvider.GetFiles().Single(file => Path.GetExtension(file) == transferFileExtension);
                }
                catch (Exception ex)
                {
                    logger.LogInformation("{ErrorMessage}", ex.Message);
                }
            });
        }

        /// <summary>
        /// Asynchronously validates the xml structure of <see cref="fileProvider"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">If <see cref="TransferFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <see cref="TransferFile"/> is <c>string.Empty</c>.</exception>
        /// <exception cref="FileNotFoundException">If <see cref="TransferFile"/> is not found.</exception>
        public async Task ValidateXmlAsync()
        {
            if (TransferFile == null) throw new ArgumentNullException(nameof(TransferFile));
            if (string.IsNullOrWhiteSpace(TransferFile)) throw new ArgumentException("Transfer file name cannot be empty.", nameof(TransferFile));

            logger.LogInformation("Validating xml structure for transfer file <{TransferFile}>", TransferFile);

            var settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                Async = true,
            };

            using var fileStream = fileProvider.OpenText(TransferFile);
            using var reader = XmlReader.Create(fileStream, settings);

            try
            {
                while (await reader.ReadAsync()) { }
            }
            catch (XmlException ex)
            {
                logger.LogWarning("Cannot parse transfer file <{TransferFile}>: {ErrorMessage}", TransferFile, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously gets the model names from a GeoPackage SQLite database.
        /// </summary>
        /// <exception cref="ArgumentNullException">If <see cref="TransferFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <see cref="TransferFile"/> is <c>string.Empty</c>.</exception>
        /// <exception cref="FileNotFoundException">If <see cref="TransferFile"/> is not found.</exception>
        internal async Task<string> ReadGpkgModelNamesAsync()
        {
            if (TransferFile == null) throw new ArgumentNullException(nameof(TransferFile));
            if (string.IsNullOrWhiteSpace(TransferFile)) throw new ArgumentException("Transfer file name cannot be empty.", nameof(TransferFile));

            logger.LogInformation("Reading model names from GeoPackage <{TransferFile}>", TransferFile);

            try
            {
                var connectionString = $"Data Source={Path.Combine(fileProvider.HomeDirectory.FullName, TransferFile)}";
                return await Task.Run(() => ReadGpkgModelNameEntries(connectionString).CleanupGpkgModelNames(configuration).Join(";"));
            }
            catch (Exception ex)
            {
                throw new GeoPackageException(
                    string.Format("Cannot read model names from the given GeoPackage SQLite database. <{0}>", ex.Message), ex);
            }
        }

        /// <summary>
        /// Asynchronously validates the <see cref="TransferFile"/> with ilivalidator/ili2gpkg.
        /// </summary>
        internal async Task ValidateAsync()
        {
            logger.LogInformation("Validating transfer file with ilivalidator/ili2gpkg");

            var rootPath = Path.Combine("${ILICHECK_UPLOADS_DIR}", Id);
            var transferFilePath = Path.Combine(rootPath, TransferFile);
            var logPath = Path.Combine(rootPath, $"{Path.GetFileNameWithoutExtension(TransferFile)}_log.log");
            var xtfLogPath = Path.Combine(rootPath, $"{Path.GetFileNameWithoutExtension(TransferFile)}_log.xtf");

            var commandPrefix = configuration.GetSection("Validation")["CommandPrefix"];
            var options = $"--log {logPath} --xtflog {xtfLogPath}";
            if (!string.IsNullOrEmpty(GpkgModelNames)) options = $"{options} --models \"{GpkgModelNames}\"";
            var command = $"{commandPrefix}ilivalidator {options} \"{transferFilePath}\"";

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
                logger.LogWarning("The ilivalidator found errors in the file. Validation failed.");
            }
            else
            {
                logger.LogInformation("The ilivalidator found no errors in the file. Validation successfull!");
            }

            logger.LogInformation("Validation completed: {Timestamp}", DateTime.Now);
        }

        /// <summary>
        /// Asynchronously cleans up any user uploaded files depending on the setting whether transfer
        /// files should be deleted.
        /// </summary>
        internal async Task CleanUploadDirectoryAsync()
        {
            var tasks = configuration
                .GetFilesToDelete(fileProvider.GetFiles(), TransferFile)
                .Select(async file =>
                {
                    logger.LogInformation("Deleting file <{File}>", file);
                    await fileProvider.DeleteFileAsync(file);
                });

            await Task.WhenAll(tasks);
        }
    }
}
