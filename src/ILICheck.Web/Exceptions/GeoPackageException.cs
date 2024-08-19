using System;

namespace ILICheck.Web
{
    /// <summary>
    /// The exception that is thrown when a GeoPackage SQLite database cannot be processed properly.
    /// </summary>
    [Serializable]
    public class GeoPackageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoPackageException"/> class.
        /// </summary>
        public GeoPackageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoPackageException"/> class
        /// with a specified error <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public GeoPackageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoPackageException"/> class
        /// with a specified error <paramref name="message"/> and a reference to the
        /// <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public GeoPackageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
