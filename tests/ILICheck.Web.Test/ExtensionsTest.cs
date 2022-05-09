using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ILICheck.Web
{
    [TestClass]
    public class ExtensionsTest
    {
        [TestMethod]
        public void CleanupGpkgModelNames()
        {
            var configItems = new Dictionary<string, string>
            {
                ["Validation:BlacklistedGpkgModels"] = "VIOLENTGLEE;CALENDARSEAGULL",
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configItems)
                .Build();

            var result = new[]
            {
                "ANGRYCHEF IRATENIGHT { REDTOTE Product Data} MAESTROTRAWL{ PEEVEDSCAN} VIOLENTGLEE {INTERLIS }",
                "SLEEPYGOPHER",
                "CALENDARSEAGULL {REV4}",
                " AUTOMAESTRO {VIOLETCALENDAR } ",
                "VIOLENTTOTE VIOLENTTOTE",
            }.CleanupGpkgModelNames(configuration);

            Assert.AreEqual("ANGRYCHEF;IRATENIGHT;MAESTROTRAWL;SLEEPYGOPHER;AUTOMAESTRO;VIOLENTTOTE", result);
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForUserUploadsWithoutGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".zip" };
            var configuration = CreateConfiguration(enableGpkgValidation: false);
            CollectionAssert.AreEqual(expected, configuration.GetAcceptedFileExtensionsForUserUploads().ToList());
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForUserUploadsWithGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".gpkg", ".zip" };
            var configuration = CreateConfiguration(enableGpkgValidation: true);
            CollectionAssert.AreEqual(expected, configuration.GetAcceptedFileExtensionsForUserUploads().ToList());
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForZipContentWithoutGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".ili" };
            var configuration = CreateConfiguration(enableGpkgValidation: false);
            CollectionAssert.AreEqual(expected, configuration.GetAcceptedFileExtensionsForZipContent().ToList());
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForZipContentWithGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".gpkg", ".ili" };
            var configuration = CreateConfiguration(enableGpkgValidation: true);
            CollectionAssert.AreEqual(expected, configuration.GetAcceptedFileExtensionsForZipContent().ToList());
        }

        [TestMethod]
        public void GetTransferFileExtensionWithoutGpkgEnabled()
        {
            var configuration = CreateConfiguration(enableGpkgValidation: false);

            // Assert transfer file extension for various zip content
            Assert.AreEqual(".XTF", configuration.GetTransferFileExtension(new[] { ".xml", ".ili", ".XTF", ".itf" }));
            Assert.AreEqual(".itf", configuration.GetTransferFileExtension(new[] { ".xml", ".ili", ".itf" }));
            Assert.AreEqual(".XML", configuration.GetTransferFileExtension(new[] { ".XML", ".ili" }));
            Assert.AreEqual(".xml", configuration.GetTransferFileExtension(new[] { ".xml" }));
            Assert.AreEqual(".itf", configuration.GetTransferFileExtension(new[] { ".itf" }));
        }

        [TestMethod]
        public void GetTransferFileExtensionWithGpkgEnabled()
        {
            var configuration = CreateConfiguration(enableGpkgValidation: true);

            // Assert transfer file extension for various zip content
            Assert.AreEqual(".xtf", configuration.GetTransferFileExtension(new[] { ".gpkg", ".xml", ".ILI", ".xtf", ".itf" }));
            Assert.AreEqual(".ITF", configuration.GetTransferFileExtension(new[] { ".GPKG", ".xml", ".ili", ".ITF" }));
            Assert.AreEqual(".xml", configuration.GetTransferFileExtension(new[] { ".gpkg", ".xml", ".ili" }));
            Assert.AreEqual(".XML", configuration.GetTransferFileExtension(new[] { ".gpkg", ".XML" }));
            Assert.AreEqual(".itf", configuration.GetTransferFileExtension(new[] { ".gpkg", ".itf" }));
            Assert.AreEqual(".gpkg", configuration.GetTransferFileExtension(new[] { ".gpkg" }));
            Assert.AreEqual(".GPkg", configuration.GetTransferFileExtension(new[] { ".GPkg" }));

            // Supports multiple model and catalogue items
            Assert.AreEqual(".itf", configuration.GetTransferFileExtension(new[] { ".itf", ".ili", ".ILI" }));
            Assert.AreEqual(".xtf", configuration.GetTransferFileExtension(new[] { ".xtf", ".xml", ".xml", ".ili", ".ili" }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Null argument should be rejected.")]
        public void GetTransferFileExtensionForNull() =>
            CreateConfiguration(enableGpkgValidation: false).GetTransferFileExtension(null);

        [TestMethod]
        [ExpectedException(typeof(TransferFileNotFoundException), "Extensions with no transfer file should be detected.")]
        public void GetTransferFileExtensionForNoTransferFile() =>
            CreateConfiguration(enableGpkgValidation: false).GetTransferFileExtension(new[] { ".ili" });

        [TestMethod]
        [ExpectedException(typeof(TransferFileNotFoundException), "Empty extensions should be detected.")]
        public void GetTransferFileExtensionForEmpty() =>
            CreateConfiguration(enableGpkgValidation: false).GetTransferFileExtension(Enumerable.Empty<string>());

        [TestMethod]
        [ExpectedException(typeof(MultipleTransferFileFoundException), "Multiple transfer file extensions of the same type should be detected.")]
        public void GetTransferFileExtensionForMultiple() =>
            CreateConfiguration(enableGpkgValidation: false).GetTransferFileExtension(new[] { ".itf", ".itf" });

        [TestMethod]
        [ExpectedException(typeof(UnknownExtensionException), "An unknown transfer file extension should be rejected.")]
        public void GetTransferFileExtensionForUnknownExtension() =>
            CreateConfiguration(enableGpkgValidation: false).GetTransferFileExtension(new[] { ".sh" });

        [TestMethod]
        [ExpectedException(typeof(UnknownExtensionException), "GeoPackage (.gpkg) should be rejected if gpkg support is disabled.")]
        public void GetTransferFileExtensionForGpkgWithoutGpkgEnabled() =>
            CreateConfiguration(enableGpkgValidation: false).GetTransferFileExtension(new[] { ".gpkg" });

        private static IConfiguration CreateConfiguration(bool enableGpkgValidation)
            => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { { "ENABLE_GPKG_VALIDATION", enableGpkgValidation.ToString() } }).Build();
    }
}
