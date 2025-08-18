using Geowerkstatt.Ilicop.Web.Ilitools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Geowerkstatt.Ilicop.Web
{
    /// <summary>
    /// Represents a health check, which is used to check the status of the ilivalidator backend service.
    /// </summary>
    public class IlivalidatorHealthCheck : IHealthCheck
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IlitoolsEnvironment ilitoolsEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="IlivalidatorHealthCheck"/> class.
        /// </summary>
        public IlivalidatorHealthCheck(IConfiguration configuration, ILogger<IlivalidatorHealthCheck> logger, IlitoolsEnvironment ilitoolsEnvironment)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.ilitoolsEnvironment = ilitoolsEnvironment;
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ilitoolsEnvironment.IsIlivalidatorInitialized)
                {
                    logger.LogError("Ilivalidator is not properly initialized.");
                    return await Task.FromResult(HealthCheckResult.Unhealthy());
                }

                if (ilitoolsEnvironment.EnableGpkgValidation && !ilitoolsEnvironment.IsIli2GpkgInitialized)
                {
                    logger.LogError("ili2gpkg expected but not initialized.");
                    return await Task.FromResult(HealthCheckResult.Degraded());
                }

                return await Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking ilivalidator health.");
                return await Task.FromResult(HealthCheckResult.Unhealthy());
            }
        }
    }
}
