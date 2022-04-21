using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// Returns a random folder path for the specified environment <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Returns a random folder path.</returns>
        public static string GetRandomFolderPath(this IConfiguration configuration)
        {
            var folderPath = configuration.GetUploadPathFormat().Replace("{Name}", Path.GetRandomFileName());
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }

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
        /// Gets the acceppted file extensions for any user uploads.
        /// </summary>
        public static IEnumerable<string> GetAcceptedFileExtensions()
        {
            var acceptedFileExtensions = new List<string> { ".xtf", ".xml", ".zip" };
            var gpkgSupportEnabled = Environment.GetEnvironmentVariable("ENABLE_GPKG_VALIDATION", EnvironmentVariableTarget.Process) == "true";
            if (gpkgSupportEnabled) acceptedFileExtensions.Add(".gpkg");
            return acceptedFileExtensions;
        }

        /// <summary>
        /// Gets a save file extension for the specified <paramref name="unsaveFileName"/>.
        /// </summary>
        public static string GetSaveFileExtensionForFileName(this string unsaveFileName) =>
            GetAcceptedFileExtensions().SingleOrDefault(extension => extension == Path.GetExtension(unsaveFileName).ToLower());

        private static string RemoveReferencedModels(this string models) =>
            removeReferencedModelsRegex.Replace(models.Trim(), string.Empty);
    }
}
