using System;

namespace Geowerkstatt.Ilicop.Web
{
    /// <summary>
    /// The exception that is thrown when an ilivalidator validation failed.
    /// </summary>
    [Serializable]
    public class ValidationFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
        /// </summary>
        public ValidationFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedException"/> class
        /// with a specified error <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ValidationFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedException"/> class
        /// with a specified error <paramref name="message"/> and a reference to the
        /// <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ValidationFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
