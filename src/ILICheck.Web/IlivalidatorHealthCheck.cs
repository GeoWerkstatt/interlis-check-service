using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;
using static ILICheck.Web.ValidatorHelper;

namespace ILICheck.Web
{
    /// <summary>
    /// Represents a health check, which is used to check the status of the ilivalidator backend service.
    /// </summary>
    public class IlivalidatorHealthCheck : IHealthCheck
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="IlivalidatorHealthCheck"/> class.
        /// </summary>
        public IlivalidatorHealthCheck(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var commandPrefix = configuration.GetSection("Validation")["CommandPrefix"];
            var command = $"{commandPrefix} ilivalidator --help".Trim();

            var exitCode = await ExecuteCommandAsync(configuration, command, cancellationToken).ConfigureAwait(false);

            if (exitCode == 0)
            {
                return await Task.FromResult(HealthCheckResult.Healthy());
            }

            return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus));
        }
    }
}
