using System;
using System.Runtime.Serialization;

namespace ILICheck.Web
{
    /// <summary>
    /// The exception that is thrown when multiple transfer files of the same type were found.
    /// </summary>
    [Serializable]
    public class MultipleTransferFileFoundException : Exception
    {
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
        /// <param name="message">The message that describes the error.</param>
        public MultipleTransferFileFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleTransferFileFoundException"/> class
        /// with a specified error <paramref name="message"/> and a reference to the
        /// <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MultipleTransferFileFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleTransferFileFoundException"/> class
        /// with serialized data.
        /// </summary>
        protected MultipleTransferFileFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
