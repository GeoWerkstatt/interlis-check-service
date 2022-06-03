using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace ILICheck.Web
{
    [TestClass]
    public class PhysicalFileProviderTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void PhysicalFileProviderForNullEnvKey() =>
            Assert.ThrowsException<ArgumentNullException>(() => new PhysicalFileProvider(CreateConfiguration(), null));

        [TestMethod]
        public void Initialize()
        {
            var physicalFileProvider = new PhysicalFileProvider(CreateConfiguration(), "ILICHECK_UPLOADS_DIR");
            physicalFileProvider.Initialize("GREENANALYST");

            var expectedHomeDirectory = Path.Combine(TestContext.DeploymentDirectory, "GREENANALYST");
            var expectedHomeDirectoryPathFormat = "${ILICHECK_UPLOADS_DIR}/GREENANALYST/";

            Assert.AreEqual(expectedHomeDirectory, physicalFileProvider.HomeDirectory.FullName);
            Assert.AreEqual(expectedHomeDirectoryPathFormat, physicalFileProvider.HomeDirectoryPathFormat);
        }

        [TestMethod]
        public void InitializeForInvalid()
        {
            var physicalFileProvider = new PhysicalFileProvider(CreateConfiguration(), "ILICHECK_UPLOADS_DIR");

            Assert.ThrowsException<ArgumentNullException>(() => physicalFileProvider.Initialize(null));
            Assert.ThrowsException<ArgumentException>(() => physicalFileProvider.Initialize(string.Empty));
        }

        private IConfiguration CreateConfiguration() =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ILICHECK_UPLOADS_DIR", TestContext.DeploymentDirectory },
            }).Build();
    }
}
