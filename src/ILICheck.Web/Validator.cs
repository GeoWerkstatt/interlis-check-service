using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static ILICheck.Web.ValidatorHelper;

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
        public async Task ExecuteAsync(string transferFile, CancellationToken cancellationToken)
        {
            if (transferFile == null) throw new ArgumentNullException(nameof(transferFile));
            if (string.IsNullOrWhiteSpace(transferFile)) throw new ArgumentException("Transfer file name cannot be empty.", nameof(transferFile));
            if (!fileProvider.Exists(transferFile)) throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Transfer file with the specified name <{0}> not found in <{1}>.", transferFile, fileProvider.HomeDirectory));

            // Set the fully qualified path to the transfer file.
            TransferFile = transferFile;

            // Unzip compressed file
            if (Path.GetExtension(TransferFile) == ".zip")
            {
                await UnzipCompressedFileAsync().ConfigureAwait(false);
            }

            // Read model names from GeoPackage
            if (Path.GetExtension(TransferFile) == ".gpkg")
            {
                GpkgModelNames = await ReadGpkgModelNamesAsync().ConfigureAwait(false);
            }

            // Additional xml validation for supported files
            var supportedExtensions = new[] { ".xml", ".xtf" };
            if (supportedExtensions.Contains(Path.GetExtension(TransferFile), StringComparer.OrdinalIgnoreCase))
            {
                await ValidateXmlAsync().ConfigureAwait(false);
            }

            // Execute validation with ilivalidator
            await ValidateAsync(cancellationToken).ConfigureAwait(false);

            // Clean up user uploaded/uncompressed files
            await CleanUploadDirectoryAsync().ConfigureAwait(false);
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
                using var archive = ZipFile.OpenRead(Path.Combine(fileProvider.HomeDirectory.FullName, TransferFile));

                var transferFileExtension = archive.Entries
                    .Select(entry => Path.GetExtension(entry.FullName))
                    .GetTransferFileExtension(configuration);

                foreach (var entry in archive.Entries)
                {
                    entry.ExtractToFile(Path.Combine(fileProvider.HomeDirectory.FullName, entry.Name));
                }

                // Set new transfer file
                TransferFile = fileProvider.GetFiles().Single(file => Path.GetExtension(file) == transferFileExtension);
            }).ConfigureAwait(false);
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
                // Validate XML file.
                while (await reader.ReadAsync().ConfigureAwait(false)) { }
            }
            catch (XmlException ex)
            {
                throw new InvalidXmlException(
                    string.Format(CultureInfo.InvariantCulture, "Cannot parse transfer file <{0}>: {1}", TransferFile, ex.Message),
                    ex);
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
                return await Task.Run(() =>
                    ReadGpkgModelNameEntries(connectionString)
                    .CleanupGpkgModelNames(configuration)
                    .JoinNonEmpty(";"))
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new GeoPackageException(
                    string.Format(CultureInfo.InvariantCulture, "Cannot read model names from the given GeoPackage SQLite database. <{0}>", ex.Message), ex);
            }
        }

        /// <summary>
        /// Asynchronously validates the <see cref="TransferFile"/> with ilivalidator/ili2gpkg.
        /// </summary>
        internal async Task ValidateAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Validating transfer file <{TransferFile}> with ilivalidator/ili2gpkg", TransferFile);

            var command = GetIlivalidatorCommand(
                configuration,
                fileProvider.HomeDirectoryPathFormat,
                TransferFile,
                GpkgModelNames);

            var exitCode = await ExecuteCommandAsync(configuration, command, cancellationToken).ConfigureAwait(false);

            if (exitCode != 0)
            {
                logger.LogInformation("The ilivalidator found errors in the file. Validation failed.");
            }
            else
            {
                logger.LogInformation("The ilivalidator found no errors in the file. Validation successful!");
            }

            logger.LogInformation("Validation completed: {Timestamp}", DateTime.Now);
        }

        /// <summary>
        /// Asynchronously cleans up any user uploaded files depending on the setting whether transfer
        /// files should be deleted.
        /// </summary>
        internal async Task CleanUploadDirectoryAsync()
        {
            var tasks = fileProvider.GetFiles()
                .GetFilesToDelete(configuration, TransferFile)
                .Select(async file =>
                {
                    logger.LogInformation("Deleting file <{File}>", file);
                    await fileProvider.DeleteFileAsync(file).ConfigureAwait(false);
                });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all available model names from a GeoPackage SQLite database.
        /// </summary>
        /// <param name="connectionString">The string used to open the connection.</param>
        /// <returns>The model names from the specified GeoPackage.</returns>
        private IEnumerable<string> ReadGpkgModelNameEntries(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var command = new SqliteCommand("SELECT * FROM T_ILI2DB_MODEL", connection);

            var reader = command.ExecuteReader();
            while (reader.Read()) yield return reader["modelName"].ToString();
        }
    }
}
