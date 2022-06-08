﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static ILICheck.Web.ValidatorHelper;

namespace ILICheck.Web
{
    /// <summary>
    /// Provides extension methods which can be reused from different locations.
    /// </summary>
    public static class Extensions
    {
        private static readonly Regex removeReferencedModelsRegex = new ("{[^}]*}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// Concatenates the members of a constructed string collection using the specified separator between each member.
        /// <c>Null</c> or empty values will be rejected.
        /// </summary>
        /// <param name="values">A collection that contains the strings to concatenate.</param>
        /// <param name="separator">The string to use as a separator.</param>
        /// <returns>A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> string.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="separator"/> is <c>null</c> or empty.</exception>
        public static string JoinNonEmpty(this IEnumerable<string> values, string separator)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (string.IsNullOrEmpty(separator)) throw new InvalidOperationException($"Null or empty {nameof(separator)} value is not allowed.");

            return string.Join(separator, values.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        /// <summary>
        /// Gets the name of the process executable.
        /// Shorthand for GetSection("Validation")["ShellExecutable"].
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The name of the process executable.</returns>
        public static string GetShellExecutable(this IConfiguration configuration) =>
            configuration.GetSection("Validation")["ShellExecutable"];

        /// <summary>
        /// Removes referenced and blacklisted model names in order to be able to successfully validate with ili2gpkg.
        /// </summary>
        /// <param name="rawGpkgModelNames">Untouched (raw) model names from a GeoPackage SQLite database (gpkg).</param>
        /// <param name="configuration">Environment configuration containing blacklisted model names.</param>
        /// <returns>A list containing valid model names for validation with ili2gpkg.</returns>
        public static IEnumerable<string> CleanupGpkgModelNames(this IEnumerable<string> rawGpkgModelNames, IConfiguration configuration)
        {
            var result = Enumerable.Empty<string>();
            var blacklistedGpkgModels = configuration.GetSection("Validation")["BlacklistedGpkgModels"].Split(';');

            foreach (var rawGpkgModelName in rawGpkgModelNames)
            {
                var models = rawGpkgModelName
                    .RemoveReferencedModels()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !blacklistedGpkgModels.Contains(x));

                result = result.Concat(models);
            }

            return result.Distinct();
        }

        /// <summary>
        /// Gets the main transfer file extension among the given file <paramref name="extensions"/>.
        /// If there are multiple transfer file extensions available in <paramref name="extensions"/>,
        /// there is a specific order defined in <see cref="ValidatorHelper.GetOrderedTransferFileExtensions"/> to choose from.
        /// </summary>
        /// <param name="extensions">All the file extensions to search for the right transfer file extension in.</param>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="UnknownExtensionException">If <paramref name="extensions"/> contains unknown extensions.</exception>
        /// <exception cref="TransferFileNotFoundException">If no transfer file was found in <paramref name="extensions"/>.</exception>
        /// <exception cref="MultipleTransferFileFoundException">If multiple transfer files were found in <paramref name="extensions"/>.</exception>
        public static string GetTransferFileExtension(this IEnumerable<string> extensions, IConfiguration configuration)
        {
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));

            // Check for unknown transfer file extensions
            foreach (var extension in extensions)
            {
                if (!GetAcceptedFileExtensionsForZipContent(configuration)
                    .Any(x => x.Contains(extension, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new UnknownExtensionException(
                        extension,
                        string.Format(CultureInfo.InvariantCulture, "Transfer file extension <{0}> is an unknown file extension.", extension));
                }
            }

            // Find transfer file among the given extensions.
            var customOrder = GetOrderedTransferFileExtensions(configuration);
            string transferFileExtension = extensions
                .Where(x => customOrder.Contains(x, StringComparer.OrdinalIgnoreCase))
                .OrderBy(x => Array.FindIndex(customOrder.ToArray(), t => t.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(transferFileExtension))
            {
                throw new TransferFileNotFoundException(string.Format(CultureInfo.InvariantCulture, "No transfer file found."));
            }
            else
            {
                // Check for multiple transfer files of the same type
                if (extensions.Count(extension => extension.Equals(transferFileExtension, StringComparison.OrdinalIgnoreCase)) > 1)
                {
                    throw new MultipleTransferFileFoundException(
                        transferFileExtension,
                        string.Format(CultureInfo.InvariantCulture, "Multiple transfer files <{0}> are not supported", transferFileExtension));
                }
                else
                {
                    return transferFileExtension;
                }
            }
        }

        /// <summary>
        /// Gets the file names from the given set of <paramref name="fileNames"/> which can be deleted after validation has been completed.
        /// </summary>
        /// <param name="fileNames">The collection of file names to get the ones which can be deleted from.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="transferFile">The transfer file name.</param>
        public static IEnumerable<string> GetFilesToDelete(this IEnumerable<string> fileNames, IConfiguration configuration, string transferFile)
        {
            if (fileNames == null) throw new ArgumentNullException(nameof(fileNames));

            if (configuration.GetValue<bool>("DELETE_TRANSFER_FILES"))
            {
                yield return transferFile;

                var logFileExtensions = new[] { ".log", ".xtf" };
                foreach (var file in fileNames)
                {
                    if (!logFileExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                        yield return file;
                }
            }

            yield break;
        }

        /// <summary>
        /// Gets the sanitized file extension for the specified <paramref name="unsafeFileName"/>.
        /// </summary>
        /// <param name="unsafeFileName">The unsafe file name.</param>
        /// <param name="acceptedFileExtensions">The accepted file extensions.</param>
        /// <returns>The sanitized file extension for the specified <paramref name="unsafeFileName"/>.</returns>
        /// <exception cref="UnknownExtensionException">If file extension of <paramref name="unsafeFileName"/> is unknown.</exception>
        public static string GetSanitizedFileExtension(this string unsafeFileName, IEnumerable<string> acceptedFileExtensions)
        {
            try
            {
                return acceptedFileExtensions
                    .Single(extension => Path.GetExtension(unsafeFileName)
                    .Equals(extension, StringComparison.OrdinalIgnoreCase));
            }
            catch (InvalidOperationException)
            {
                var invalidFileExtension = Path.GetExtension(unsafeFileName);
                throw new UnknownExtensionException(
                    invalidFileExtension,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Transfer file extension <{0}> is an unknown file extension.",
                        invalidFileExtension));
            }
        }

        /// <summary>
        /// Gets the log file for the specified <paramref name="logType"/>.
        /// </summary>
        /// <param name="fileProvider">The file provider which provides read/write access to a predefined folder.</param>
        /// <param name="logType">The log type.</param>
        /// <returns>The log file for the specified <paramref name="logType"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="fileProvider"/> is <c>null</c>.</exception>
        /// <exception cref="FileNotFoundException">If the log file could not be found.</exception>
        public static string GetLogFile(this IFileProvider fileProvider, LogType logType)
        {
            if (fileProvider == null) throw new ArgumentNullException(nameof(fileProvider));

            try
            {
                return fileProvider.GetFiles().Where(x => x.EndsWith($"_log.{logType}", StringComparison.OrdinalIgnoreCase)).Single();
            }
            catch (InvalidOperationException)
            {
                throw new FileNotFoundException(
                    string.Format(CultureInfo.InvariantCulture, "Log file of type <{0}> not found in <{1}>", logType, fileProvider.HomeDirectory));
            }
        }

        /// <summary>
        /// Asynchronously invokes the specified <paramref name="action"/> on the given items in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items in the sequence.</typeparam>
        /// <param name="source">The asynchronous sequence containing the items.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the asynchronous operation.</param>
        public static async Task ParallelForEachAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task> action, CancellationToken cancellationToken = default)
        {
            var dataFlowBlockOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                CancellationToken = cancellationToken,
            };

            var actionBlock = new ActionBlock<T>(action, dataFlowBlockOptions);
            await foreach (var item in source)
            {
                await actionBlock.SendAsync(item);
            }

            actionBlock.Complete();
            await actionBlock.Completion;
        }

        private static string RemoveReferencedModels(this string models) =>
            removeReferencedModelsRegex.Replace(models.Trim(), string.Empty);
    }
}
