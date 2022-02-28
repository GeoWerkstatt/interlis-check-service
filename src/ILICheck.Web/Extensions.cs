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

        private static string RemoveReferencedModels(this string models) =>
            removeReferencedModelsRegex.Replace(models.Trim(), string.Empty);
    }
}
