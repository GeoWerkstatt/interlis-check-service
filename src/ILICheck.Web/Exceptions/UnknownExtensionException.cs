using System;
using System.Runtime.Serialization;

namespace ILICheck.Web
{
    /// <summary>
    /// The exception that is thrown when a unknown or invalid file extension have been found.
    /// </summary>
    [Serializable]
    public class UnknownExtensionException : Exception
    {
        /// <summary>
        /// Gets the unknown or invalid file extension.
        /// </summary>
        public string FileExtension { get; }

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
        /// <param name="extension">The unknown or invalid file extension.</param>
        /// <param name="message">The message that describes the error.</param>
        public UnknownExtensionException(string extension, string message)
            : base(message)
        {
            FileExtension = extension;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownExtensionException"/> class
        /// with a specified error <paramref name="message"/> and a reference to the
        /// <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="fileExtension">The unknown or invalid file extension.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UnknownExtensionException(string fileExtension, string message, Exception innerException)
            : base(message, innerException)
        {
            FileExtension = fileExtension;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownExtensionException"/> class
        /// with serialized data.
        /// </summary>
        protected UnknownExtensionException(SerializationInfo info, StreamingContext streamingContext)
            : base(info, streamingContext)
        {
            FileExtension = info.GetString(nameof(FileExtension));
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(FileExtension), FileExtension);
            base.GetObjectData(info, context);
        }
    }
}
