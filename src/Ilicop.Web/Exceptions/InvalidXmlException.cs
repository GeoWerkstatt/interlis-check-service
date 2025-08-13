using System;

namespace Geowerkstatt.Ilicop.Web
{
    /// <summary>
    /// The exception that is thrown when a XML transfer file cannot be parsed correctly.
    /// </summary>
    [Serializable]
    public class InvalidXmlException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidXmlException"/> class.
        /// </summary>
        public InvalidXmlException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidXmlException"/> class
        /// with a specified error <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidXmlException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidXmlException"/> class
        /// with a specified error <paramref name="message"/> and a reference to the
        /// <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InvalidXmlException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
