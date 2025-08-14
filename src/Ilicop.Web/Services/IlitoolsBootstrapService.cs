using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Geowerkstatt.Ilicop.Web.Services
{
    /// <summary>
    /// Service responsible for bootstrapping and managing ilitools.
    /// </summary>
    public partial class IlitoolsBootstrapService : IHostedService
    {
        private readonly ILogger<IlitoolsBootstrapService> logger;
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;

        private readonly string ilitoolsHomeDir;
        private readonly string ilitoolsCacheDir;

        public IlitoolsBootstrapService(ILogger<IlitoolsBootstrapService> logger, IConfiguration configuration, HttpClient httpClient)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.httpClient = httpClient;

            ilitoolsHomeDir = configuration.GetValue<string>("ILITOOLS_HOME_DIR") ?? "/ilitools";
            ilitoolsCacheDir = configuration.GetValue<string>("ILITOOLS_CACHE_DIR") ?? "/cache";
        }

        /// <summary>
        /// Starts the service and bootstraps the ilitools.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting ilitools bootstrap service");

            try
            {
                // Set cache environment variable for ilitools
                Environment.SetEnvironmentVariable("ILI_CACHE", ilitoolsCacheDir);
                logger.LogDebug("Set ILI_CACHE environment variable to: {IlitoolsCacheDir}", ilitoolsCacheDir);

                // Bootstrap ilivalidator
                await InitializeIlitoolAsync("ilivalidator", "ILIVALIDATOR_VERSION", GetLatestIlivalidatorVersionAsync, cancellationToken);

                // Bootstrap ili2gpkg if GPKG validation is enabled
                var enableGpkgValidation = configuration.GetValue<bool>("ENABLE_GPKG_VALIDATION");
                if (enableGpkgValidation)
                {
                    await InitializeIlitoolAsync("ili2gpkg", "ILI2GPKG_VERSION", GetLatestIli2gpkgVersionAsync, cancellationToken);
                }
                else
                {
                    logger.LogInformation("GPKG validation is disabled, skipping ili2gpkg bootstrap.");
                }

                logger.LogInformation("Ilitools successfully initialized!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to bootstrap ilitools.");
                throw new InvalidOperationException("Ilitools initialization failed.", ex);
            }
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Initializes the given ilitool.
        /// </summary>
        private async Task InitializeIlitoolAsync(string ilitoolName, string ilitoolVersionConfigKey, Func<CancellationToken, Task<string>> getLatestToolVersionAsync, CancellationToken cancellationToken)
        {
            var version = configuration.GetValue<string>(ilitoolVersionConfigKey);
            if (string.IsNullOrEmpty(version))
            {
                logger.LogInformation("{VersionKey} not specified, fetching latest version.", ilitoolVersionConfigKey);

                version = await getLatestToolVersionAsync(cancellationToken);
                if (string.IsNullOrEmpty(version))
                {
                    logger.LogWarning("Failed to fetch the latest {Ilitool} version. Falling back to the latest installed version.", ilitoolName);
                    version = GetLatestInstalledIlitoolVersion(ilitoolName);
                }
            }

            if (string.IsNullOrEmpty(version))
            {
                throw new InvalidOperationException($"Unable to determine {ilitoolName} version.");
            }

            await DownloadAndConfigureIlitoolAsync(ilitoolName, version, cancellationToken);

            // Export version for other parts of the application
            Environment.SetEnvironmentVariable(ilitoolVersionConfigKey, version);
            logger.LogInformation("{Ilitool} version {Version} initialized successfully.", ilitoolName, version);
        }

        /// <summary>
        /// Downloads and configures the specified ilitool.
        /// </summary>
        private async Task DownloadAndConfigureIlitoolAsync(string ilitool, string version, CancellationToken cancellationToken)
        {
            // Exit if the tool is already installed and valid
            var installDir = Path.Combine(ilitoolsHomeDir, ilitool, version);
            if (Directory.Exists(installDir))
            {
                logger.LogInformation("{Ilitool}-{Version} is already installed. Skipping download and configuration.", ilitool, version);
                return;
            }

            logger.LogInformation("Download and configure {Ilitool}-{Version}...", ilitool, version);

            try
            {
                // Ensure the ilitools home directory exists
                if (!Directory.Exists(ilitoolsHomeDir))
                {
                    Directory.CreateDirectory(ilitoolsHomeDir);
                    logger.LogDebug("Created ilitools home directory: {IlitoolsHomeDir}", ilitoolsHomeDir);
                }

                var downloadUrl = new UriBuilder($"https://downloads.interlis.ch/{ilitool}/{ilitool}-{version}.zip");
                var tempFilePath = Path.GetTempFileName();

                // Download the zip file
                logger.LogDebug("Downloading {Ilitool} from {DownloadUrl}", ilitool, downloadUrl);
                using var response = await httpClient.GetAsync(downloadUrl.Uri.AbsoluteUri, cancellationToken);
                response.EnsureSuccessStatusCode();

                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    await response.Content.CopyToAsync(fileStream, cancellationToken);
                }

                logger.LogDebug(
                    "Downloaded {Ilitool} zip file to {TempFilePath} ({Size} bytes)",
                    ilitool,
                    tempFilePath,
                    new FileInfo(tempFilePath).Length);

                // Create install directory
                if (Directory.Exists(installDir))
                {
                    Directory.Delete(installDir, recursive: true);
                    logger.LogDebug("Cleaned existing install directory: {InstallDir}", installDir);
                }

                Directory.CreateDirectory(installDir);
                logger.LogDebug("Created install directory: {InstallDir}", installDir);

                // Extract the zip file
                ZipFile.ExtractToDirectory(tempFilePath, installDir, overwriteFiles: true);
                logger.LogDebug("Extracted {Ilitool} to {InstallDir}", ilitool, installDir);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not install {Ilitool}-{Version}.", ilitool, version);
                throw;
            }
        }

        /// <summary>
        /// Gets the latest ilivalidator version from the web.
        /// </summary>
        private async Task<string> GetLatestIlivalidatorVersionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await httpClient.GetStringAsync("https://www.interlis.ch/downloads/ilivalidator", cancellationToken);
                var match = Regex.Match(response, @"(?<=ilivalidator-)\d+\.\d+\.\d+");
                return match.Success ? match.Value : null;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch latest ilivalidator version from web.");
                return null;
            }
        }

        /// <summary>
        /// Gets the latest ili2gpkg version from the web.
        /// </summary>
        private async Task<string> GetLatestIli2gpkgVersionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await httpClient.GetStringAsync("https://www.interlis.ch/downloads/ili2db", cancellationToken);
                var match = Regex.Match(response, @"(?<=ili2gpkg-)\d+\.\d+\.\d+");
                return match.Success ? match.Value : null;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch latest ili2gpkg version from web");
                return null;
            }
        }

        /// <summary>
        /// Gets the latest installed version of the specified ilitool.
        /// </summary>
        internal string GetLatestInstalledIlitoolVersion(string ilitool)
        {
            try
            {
                var toolDir = Path.Combine(ilitoolsHomeDir, ilitool);
                if (!Directory.Exists(toolDir))
                {
                    logger.LogDebug("Tool directory does not exist: {ToolDir}", toolDir);
                    return null;
                }

                var versions = Directory.GetDirectories(toolDir)
                    .Select(Path.GetFileName)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .OrderBy(v => new Version(v))
                    .ToList();

                if (versions.Count == 0)
                {
                    logger.LogDebug("No valid installed versions found for {Ilitool}.", ilitool);
                    return null;
                }

                var latestVersion = versions.LastOrDefault() ?? null;
                logger.LogDebug("Latest installed version for {Ilitool}: {Version}", ilitool, latestVersion);
                return latestVersion;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to get latest installed version for {Ilitool}.", ilitool);
                return null;
            }
        }
    }
}
