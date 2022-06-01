using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ILICheck.Web
{
    /// <summary>
    /// Provides read/write access to files in a predefined folder.
    /// </summary>
    public class PhysicalFileProvider : IFileProvider
    {
        private readonly IConfiguration configuration;
        private readonly string rootDirectoryEnvironmentKey;

        private bool initialized;

        /// <inheritdoc/>
        public DirectoryInfo HomeDirectory { get; private set; }

        /// <inheritdoc/>
        public string HomeDirectoryPathFormat { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileProvider"/> at the given root directory path.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="variable">The name of the environment variable containing the root directory path.</param>
        public PhysicalFileProvider(IConfiguration configuration, string variable)
        {
            this.configuration = configuration;
            rootDirectoryEnvironmentKey = variable;
        }

        /// <inheritdoc/>
        public FileStream CreateFile(string file)
        {
            if (!initialized) throw new InvalidOperationException("The file provider needs to be initialized first.");
            return File.Create(Path.Combine(HomeDirectory.FullName, file));
        }

        /// <inheritdoc/>
        public StreamReader OpenText(string file)
        {
            if (!initialized) throw new InvalidOperationException("The file provider needs to be initialized first.");
            return File.OpenText(Path.Combine(HomeDirectory.FullName, file));
        }

        /// <inheritdoc/>
        public virtual Task DeleteFileAsync(string file)
        {
            if (!initialized) throw new InvalidOperationException("The file provider needs to be initialized first.");
            return Task.Run(() => File.Delete(Path.Combine(HomeDirectory.FullName, file)));
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> GetFiles()
        {
            if (!initialized) throw new InvalidOperationException("The file provider needs to be initialized first.");
            return HomeDirectory.GetFiles().Select(x => x.Name);
        }

        /// <inheritdoc/>
        public void Initialize(string name)
        {
            name = Path.TrimEndingDirectorySeparator(name);
            HomeDirectory = new DirectoryInfo(configuration.GetValue<string>(rootDirectoryEnvironmentKey)).CreateSubdirectory(name);
            HomeDirectoryPathFormat = string.Format(CultureInfo.InvariantCulture, "${{{0}}}/{1}/", rootDirectoryEnvironmentKey, name);

            initialized = true;
        }
    }
}
