using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using static Geowerkstatt.Ilicop.Web.ValidatorHelper;

namespace Geowerkstatt.Ilicop.Web
{
    /// <summary>
    /// Represents a health check, which is used to check the status of the ilivalidator backend service.
    /// </summary>
    public class IlivalidatorHealthCheck : IHealthCheck
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IlivalidatorHealthCheck"/> class.
        /// </summary>
        public IlivalidatorHealthCheck(IConfiguration configuration, ILogger<IlivalidatorHealthCheck> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var ilivalidatorVersion = Environment.GetEnvironmentVariable("ILIVALIDATOR_VERSION");
                if (string.IsNullOrEmpty(ilivalidatorVersion))
                {
                    logger.LogError("Ilivalidator is not properly initialized.");
                    return await Task.FromResult(HealthCheckResult.Unhealthy());
                }

                // Smoke test ilivalidator by checking its version; the output is discarded and not printed to the console.
                var commandFormat = configuration.GetSection("Validation")["CommandFormat"];
                var command = string.Format(CultureInfo.InvariantCulture, commandFormat, "ilivalidator --version > NUL 2>&1");

                var exitCode = await ExecuteCommandAsync(configuration, command, cancellationToken).ConfigureAwait(false);
                if (exitCode == 0)
                {
                    return await Task.FromResult(HealthCheckResult.Healthy());
                }

                return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking ilivalidator health.");
                return await Task.FromResult(HealthCheckResult.Unhealthy());
            }
        }
    }
}
