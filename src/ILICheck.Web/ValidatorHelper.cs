using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ILICheck.Web
{
    /// <summary>
    /// Provides some <see cref="Validator"/> helper methods.
    /// </summary>
    public static class ValidatorHelper
    {
        /// <summary>
        /// Gets the accepted file extensions for user web uploads.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public static IEnumerable<string> GetAcceptedFileExtensionsForUserUploads(IConfiguration configuration)
        {
            var additionalExtensions = new[] { ".zip" };
            return GetOrderedTransferFileExtensions(configuration).Concat(additionalExtensions);
        }

        /// <summary>
        /// Gets the accepted file extensions for ZIP content.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public static IEnumerable<string> GetAcceptedFileExtensionsForZipContent(IConfiguration configuration)
        {
            var additionalExtensions = new[] { ".ili" };
            return GetOrderedTransferFileExtensions(configuration).Concat(additionalExtensions);
        }

        /// <summary>
        /// Gets the transfer file extensions which are supported for validation with ilivalidator.
        /// The ordered list of transfer file extensions is prioritized according to known rules
        /// when validate with additional files (eg. catalogues).
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public static IEnumerable<string> GetOrderedTransferFileExtensions(IConfiguration configuration)
        {
            foreach (var extension in new[] { ".xtf", ".itf", ".xml" })
            {
                yield return extension;
            }

            if (configuration.GetValue<bool>("ENABLE_GPKG_VALIDATION")) yield return ".gpkg";
        }

        /// <summary>
        /// Gets the full command which can be executed on a shell in an Unix environment to validate
        /// the specified <paramref name="transferFile"/> with ilivalidator including environment specific
        /// command prefixes if applicable. The path created by this function uses the Unix path format.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="homeDirectory">The home directory path using the Unix path format.</param>
        /// <param name="transferFile">The transfer file name.</param>
        /// <param name="gpkgModelNames">The extracted model names. Optional.</param>
        /// <returns>The ilivalidator command.</returns>
        public static string GetIlivalidatorCommand(IConfiguration configuration, string homeDirectory, string transferFile, string gpkgModelNames = null)
        {
            homeDirectory = Path.TrimEndingDirectorySeparator(homeDirectory);
            var transferFileNameWithoutExtension = Path.GetFileNameWithoutExtension(transferFile);

            var logPath = $"{homeDirectory}/{transferFileNameWithoutExtension}_log.log";
            var xtfLogPath = $"{homeDirectory}/{transferFileNameWithoutExtension}_log.xtf";
            var transferFilePath = $"{homeDirectory}/{transferFile}";
            var commandFormat = configuration.GetSection("Validation")["CommandFormat"];
            var options = $"--log \"{logPath}\" --xtflog \"{xtfLogPath}\" --verbose";
            if (!string.IsNullOrEmpty(gpkgModelNames)) options = $"{options} --models \"{gpkgModelNames}\"";

            return string.Format(
                CultureInfo.InvariantCulture,
                commandFormat,
                $"ilivalidator {options} \"{transferFilePath}\"");
        }

        /// <summary>
        /// Asynchronously executes the given <paramref name="command"/> on the shell specified in <see cref="Extensions.GetShellExecutable(IConfiguration)"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the asynchronous operation.</param>
        /// <returns>The exit code that the associated process specified when it terminated.</returns>
        public static async Task<int> ExecuteCommandAsync(IConfiguration configuration, string command, CancellationToken cancellationToken = default)
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = configuration.GetShellExecutable(),
                    Arguments = command,
                    UseShellExecute = true,
                },
                EnableRaisingEvents = true,
            };

            process.Start();

            try
            {
                await process.WaitForExitAsync(cancellationToken);
                return process.ExitCode;
            }
            catch (OperationCanceledException)
            {
                process.Kill();
                return 1;
            }
        }
    }
}
