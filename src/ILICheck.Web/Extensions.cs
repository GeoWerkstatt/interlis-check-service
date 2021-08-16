using Microsoft.Extensions.Configuration;

namespace ILICheck.Web
{
    public static class Extensions
    {
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
        /// Shorthand for GetSection("Validation")["ProcessExecutable"].
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The name of the process executable.</returns>
        public static string GetProcessExecutable(this IConfiguration configuration) =>
            configuration.GetSection("Validation")["ProcessExecutable"];
    }
}
