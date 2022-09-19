using System;
using System.Threading;
using System.Threading.Tasks;

namespace ILICheck.Web
{
    /// <summary>
    /// Provides methods to validate INTERLIS transfer files.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Gets the identifier for this instance.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the home directory for all the files releated to this instance.
        /// </summary>
        string HomeDirectory { get; }

        /// <summary>
        /// Gets the transfer file name.
        /// </summary>
        string TransferFile { get; }

        /// <summary>
        /// Gets the extracted model names.
        /// </summary>
        /// <remarks>Only applicable if <see cref="TransferFile"/> is a GeoPackage.</remarks>
        string GpkgModelNames { get; }

        /// <summary>
        /// Asynchronously validates the <paramref name="transferFile"/> specified.
        /// The transfer file must already be located in the <see cref="HomeDirectory"/>
        /// when executing this function.
        /// </summary>
        /// <param name="transferFile">The name of the transfer file to validate.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="transferFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="transferFile"/> is <c>string.Empty</c>.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="transferFile"/> is not found.</exception>
        Task ExecuteAsync(string transferFile, CancellationToken cancellationToken);
    }
}
