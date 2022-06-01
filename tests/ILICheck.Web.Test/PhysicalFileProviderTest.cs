using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace ILICheck.Web
{
    [TestClass]
    public class PhysicalFileProviderTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void HomeDirectory()
        {
            var rootDirectoryEnvironmentKey = "UPSALA_TRALLALA";
            var physicalFileProvider = new PhysicalFileProvider(
                CreateConfiguration(rootDirectoryEnvironmentKey),
                rootDirectoryEnvironmentKey);

            physicalFileProvider.Initialize("SLEEPYFARM");

            Assert.AreEqual(
                Path.Combine(TestContext.DeploymentDirectory, "SLEEPYFARM"),
                physicalFileProvider.HomeDirectory.FullName);
        }

        [TestMethod]
        public void HomeDirectoryPathFormat()
        {
            var rootDirectoryEnvironmentKey = "SPORKBOUNCE_IRATEFIRE";
            var physicalFileProvider = new PhysicalFileProvider(
                CreateConfiguration(rootDirectoryEnvironmentKey),
                rootDirectoryEnvironmentKey);

            physicalFileProvider.Initialize("CHILLYMOON");

            Assert.AreEqual(
                "${SPORKBOUNCE_IRATEFIRE}/CHILLYMOON/",
                physicalFileProvider.HomeDirectoryPathFormat);
        }

        private IConfiguration CreateConfiguration(string rootDirectoryEnvironmentKey = null) =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { rootDirectoryEnvironmentKey, TestContext.DeploymentDirectory },
            }).Build();
    }
}
