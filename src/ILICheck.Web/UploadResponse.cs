using System;

namespace ILICheck.Web
{
    /// <summary>
    /// The upload response type.
    /// </summary>
    public class UploadResponse
    {
        /// <summary>
        /// Gets or sets the job identification.
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// The status url.
        /// </summary>
        public Uri StatusUrl { get; set; }
    }
}
