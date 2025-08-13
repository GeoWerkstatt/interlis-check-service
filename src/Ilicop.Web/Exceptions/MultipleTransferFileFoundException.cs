using System;

namespace Geowerkstatt.Ilicop.Web
{
    /// <summary>
    /// The exception that is thrown when multiple transfer files of the same type were found.
    /// </summary>
    [Serializable]
    public class MultipleTransferFileFoundException : Exception
    {
        /// <summary>
        /// Gets the transfer file extension which was found multiple times.
        /// </summary>
        public string FileExtension { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleTransferFileFoundException"/> class.
        /// </summary>
        public MultipleTransferFileFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleTransferFileFoundException"/> class
        /// with a specified error <paramref name="message"/>.
        /// </summary>
        /// <param name="fileExtension">The transfer file extension which was found multiple times.</param>
        /// <param name="message">The message that describes the error.</param>
        public MultipleTransferFileFoundException(string fileExtension, string message)
            : base(message)
        {
            FileExtension = fileExtension;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleTransferFileFoundException"/> class
        /// with a specified error <paramref name="message"/> and a reference to the
        /// <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="fileExtension">The transfer file extension which was found multiple times.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MultipleTransferFileFoundException(string fileExtension, string message, Exception innerException)
            : base(message, innerException)
        {
            FileExtension = fileExtension;
        }
    }
}
