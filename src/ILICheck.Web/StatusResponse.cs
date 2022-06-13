using System;

namespace ILICheck.Web
{
    /// <summary>
    /// The status response type.
    /// </summary>
    public class StatusResponse
    {
        /// <summary>
        /// Gets or sets the job identification.
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Gets or sets the job status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// Gets or sets the job status message.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the log url.
        /// </summary>
        public Uri LogUrl { get; set; }

        /// <summary>
        /// Gets or sets the XTF log url.
        /// </summary>
        public Uri XtfLogUrl { get; set; }
    }
}
