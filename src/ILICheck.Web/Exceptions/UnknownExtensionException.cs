using System;

namespace ILICheck.Web
{
    /// <summary>
    /// The exception that is thrown when a unknown or invalid file extension have been found.
    /// </summary>
    public class UnknownExtensionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownExtensionException"/> class.
        /// </summary>
        public UnknownExtensionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownExtensionException"/> class
        /// with a specified error <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UnknownExtensionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownExtensionException"/> class
        /// with a specified error <paramref name="message"/> and a reference to the
        /// <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UnknownExtensionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
