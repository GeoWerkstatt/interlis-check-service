using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ILICheck.Web
{
    public static class Extensions
    {
        private static readonly Regex removeReferencedModelsRegex = new ("{[^}]*}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// Shorthand for GetSection("Upload")["PathFormat"].
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The Upload PathFormat.</returns>
        public static string GetUploadPathFormat(this IConfiguration configuration) =>
            configuration.GetSection("Upload")["PathFormat"];

        /// <summary>
        /// Gets the Upload Path for the specified <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="connectionId">The connection id.</param>
        /// <returns>The Upload Path for the specified <paramref name="connectionId"/>.</returns>
        public static string GetUploadPathForSession(this IConfiguration configuration, string connectionId) =>
            configuration.GetUploadPathFormat().Replace("{Name}", connectionId);

        /// <summary>
        /// Shorthand for GetSection("Validation")["ShellExecutable"].
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The name of the process executable.</returns>
        public static string GetShellExecutable(this IConfiguration configuration) =>
            configuration.GetSection("Validation")["ShellExecutable"];

        /// <summary>
        /// Gets all available model names from a GeoPackage SQLite database.
        /// </summary>
        public static IEnumerable<string> ReadGpkgModelNameEntries(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var command = new SqliteCommand("SELECT * FROM T_ILI2DB_MODEL", connection);

            var reader = command.ExecuteReader();
            while (reader.Read()) yield return reader["modelName"].ToString();
        }

        /// <summary>
        /// Removes referenced and blacklisted model names in order to be able to successfully validate with ili2gpkg.
        /// </summary>
        /// <param name="rawGpkgModelNames">Untouched (raw) model names from a GeoPackage SQLite database (gpkg).</param>
        /// <param name="configuration">Environment configuration containing blacklisted model names.</param>
        /// <returns>A list containing valid model names for validation with ili2gpkg.</returns>
        public static string CleanupGpkgModelNames(this IEnumerable<string> rawGpkgModelNames, IConfiguration configuration)
        {
            var result = Enumerable.Empty<string>();
            var blacklistedGpkgModels = configuration.GetSection("Validation")["BlacklistedGpkgModels"].Split(';');

            foreach (var rawGpkgModelName in rawGpkgModelNames)
            {
                var models = rawGpkgModelName
                    .RemoveReferencedModels()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(m => !blacklistedGpkgModels.Contains(m));

                result = result.Concat(models);
            }

            return string.Join(';', result.Distinct());
        }

        /// <summary>
        /// Gets the accepted file extensions for user web uploads.
        /// </summary>
        public static IEnumerable<string> GetAcceptedFileExtensionsForUserUploads()
        {
            var additionalExtensions = new[] { ".zip" };
            foreach (var extension in GetOrderedTransferFileExtensions().Concat(additionalExtensions))
            {
                yield return extension;
            }
        }

        /// <summary>
        /// Gets the accepted file extensions for ZIP content.
        /// </summary>
        public static IEnumerable<string> GetAcceptedFileExtensionsForZipContent()
        {
            var additionalExtensions = new[] { ".ili" };
            foreach (var extension in GetOrderedTransferFileExtensions().Concat(additionalExtensions))
            {
                yield return extension;
            }
        }

        /// <summary>
        /// Gets the transfer file extension for the given file <paramref name="extensions"/>.
        /// If there are multiple transfer file extensions available in <paramref name="extensions"/>,
        /// there is a specific order defined in <see cref="GetOrderedTransferFileExtensions"/> to choose from.
        /// </summary>
        /// <exception cref="UnknownExtensionException">If <paramref name="extensions"/> contains unknown extensions.</exception>
        /// <exception cref="TransferFileNotFoundException">If no transfer file was found in <paramref name="extensions"/>.</exception>
        /// <exception cref="MultipleTransferFileFoundException">If multiple transfer files were found in <paramref name="extensions"/>.</exception>
        public static string GetTransferFileExtension(this IEnumerable<string> extensions)
        {
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));

            // Check for unknown transfer file extensions
            foreach (var extension in extensions)
            {
                if (!GetAcceptedFileExtensionsForZipContent()
                    .Any(x => x.Contains(extension, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new UnknownExtensionException(
                        string.Format("Transfer file extension <{0}> is an unknown file extension.", extension));
                }
            }

            // Find transfer file among the given extensions.
            var customOrder = GetOrderedTransferFileExtensions();
            string transferFileExtension = extensions
                .OrderBy(x => Array.FindIndex(customOrder.ToArray(), t => t.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .Where(x => customOrder.Contains(x, StringComparer.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(transferFileExtension))
            {
                throw new TransferFileNotFoundException(string.Format("No transfer file found."));
            }
            else
            {
                // Check for multiple transfer files of the same type
                if (extensions.Count(extension => extension.Equals(transferFileExtension, StringComparison.OrdinalIgnoreCase)) > 1)
                {
                    throw new MultipleTransferFileFoundException(string.Format("Multiple transfer files <{0}> are not supported", transferFileExtension));
                }
                else
                {
                    return transferFileExtension;
                }
            }
        }

        /// <summary>
        /// Gets the transfer file extensions which are supported for validation with ilivalidator.
        /// The ordered list of transfer file extensions is prioritized according to known rules
        /// when validate with additional files (eg. catalogues).
        /// </summary>
        private static IEnumerable<string> GetOrderedTransferFileExtensions()
        {
            var acceptedFileExtensions = new List<string> { ".xtf", ".itf", ".xml" };
            var gpkgSupportEnabled = Environment
                .GetEnvironmentVariable("ENABLE_GPKG_VALIDATION", EnvironmentVariableTarget.Process) == "true";

            if (gpkgSupportEnabled) acceptedFileExtensions.Add(".gpkg");
            return acceptedFileExtensions;
        }

        private static string RemoveReferencedModels(this string models) =>
            removeReferencedModelsRegex.Replace(models.Trim(), string.Empty);
    }
}
