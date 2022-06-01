using System;
using System.Collections.Generic;
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
        private bool initialized;

        /// <inheritdoc/>
        public DirectoryInfo HomeDirectory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileProvider"/>
        /// at the given <paramref name="root"/> directory path.
        /// </summary>
        /// <param name="root">The root directory. This must be an absolute path.</param>
        public PhysicalFileProvider(string root)
        {
            HomeDirectory = new DirectoryInfo(root);
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
            HomeDirectory = HomeDirectory.CreateSubdirectory(name);
            initialized = true;
        }
    }
}
