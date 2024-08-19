using System;

namespace ILICheck.Web
{
    /// <summary>
    /// The exception that is thrown when no transfer file was found.
    /// </summary>
    [Serializable]
    public class TransferFileNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransferFileNotFoundException"/> class.
        /// </summary>
        public TransferFileNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferFileNotFoundException"/> class
        /// with a specified error <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TransferFileNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferFileNotFoundException"/> class
        /// with a specified error <paramref name="message"/> and a reference to the
        /// <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TransferFileNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
