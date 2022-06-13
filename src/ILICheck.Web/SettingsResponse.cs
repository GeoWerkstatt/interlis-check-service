using System;

namespace ILICheck.Web
{
    /// <summary>
    /// The settings response type.
    /// </summary>
    public class SettingsResponse
    {
        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the application version.
        /// </summary>
        public string ApplicationVersion { get; set; }

        /// <summary>
        /// Gets or sets the vendor link.
        /// </summary>
        public string VendorLink { get; set; }

        /// <summary>
        /// Gets or sets the ilivalidator version.
        /// </summary>
        public string IlivalidatorVersion { get; set; }

        /// <summary>
        /// Gets or sets the ili2gpkg version.
        /// </summary>
        public string Ili2gpkgVersion { get; set; }

        /// <summary>
        /// Gets or sets the accepted file types.
        /// </summary>
        public string AcceptedFileTypes { get; set; }
    }
}
