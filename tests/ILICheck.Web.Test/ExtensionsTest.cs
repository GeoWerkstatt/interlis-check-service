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
            Environment.SetEnvironmentVariable("ENABLE_GPKG_VALIDATION", "false");
            CollectionAssert.AreEqual(expected, Extensions.GetAcceptedFileExtensionsForUserUploads().ToList());
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForUserUploadsWithGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".gpkg", ".zip" };
            Environment.SetEnvironmentVariable("ENABLE_GPKG_VALIDATION", "true");
            CollectionAssert.AreEqual(expected, Extensions.GetAcceptedFileExtensionsForUserUploads().ToList());
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForZipContentWithoutGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".ili" };
            Environment.SetEnvironmentVariable("ENABLE_GPKG_VALIDATION", "false");
            CollectionAssert.AreEqual(expected, Extensions.GetAcceptedFileExtensionsForZipContent().ToList());
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForZipContentWithGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".gpkg", ".ili" };
            Environment.SetEnvironmentVariable("ENABLE_GPKG_VALIDATION", "true");
            CollectionAssert.AreEqual(expected, Extensions.GetAcceptedFileExtensionsForZipContent().ToList());
        }

        [TestMethod]
        public void GetTransferFileExtensionWithoutGpkgEnabled()
        {
            // Assert transfer file extension for various zip content
            Environment.SetEnvironmentVariable("ENABLE_GPKG_VALIDATION", "false");
            Assert.AreEqual(".XTF", Extensions.GetTransferFileExtension(new[] { ".xml", ".ili", ".XTF", ".itf" }));
            Assert.AreEqual(".itf", Extensions.GetTransferFileExtension(new[] { ".xml", ".ili", ".itf" }));
            Assert.AreEqual(".XML", Extensions.GetTransferFileExtension(new[] { ".XML", ".ili" }));
            Assert.AreEqual(".xml", Extensions.GetTransferFileExtension(new[] { ".xml" }));
            Assert.AreEqual(".itf", Extensions.GetTransferFileExtension(new[] { ".itf" }));
        }

        [TestMethod]
        public void GetTransferFileExtensionWithGpkgEnabled()
        {
            // Assert transfer file extension for various zip content
            Environment.SetEnvironmentVariable("ENABLE_GPKG_VALIDATION", "true");
            Assert.AreEqual(".xtf", Extensions.GetTransferFileExtension(new[] { ".gpkg", ".xml", ".ILI", ".xtf", ".itf" }));
            Assert.AreEqual(".ITF", Extensions.GetTransferFileExtension(new[] { ".GPKG", ".xml", ".ili", ".ITF" }));
            Assert.AreEqual(".xml", Extensions.GetTransferFileExtension(new[] { ".gpkg", ".xml", ".ili" }));
            Assert.AreEqual(".XML", Extensions.GetTransferFileExtension(new[] { ".gpkg", ".XML" }));
            Assert.AreEqual(".itf", Extensions.GetTransferFileExtension(new[] { ".gpkg", ".itf" }));
            Assert.AreEqual(".gpkg", Extensions.GetTransferFileExtension(new[] { ".gpkg" }));
            Assert.AreEqual(".GPkg", Extensions.GetTransferFileExtension(new[] { ".GPkg" }));

            // Supports multiple model and catalogue items
            Assert.AreEqual(".itf", Extensions.GetTransferFileExtension(new[] { ".itf", ".ili", ".ILI" }));
            Assert.AreEqual(".xtf", Extensions.GetTransferFileExtension(new[] { ".xtf", ".xml", ".xml", ".ili", ".ili" }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Null argument should be rejected.")]
        public void GetTransferFileExtensionForNull() => Extensions.GetTransferFileExtension(null);

        [TestMethod]
        [ExpectedException(typeof(TransferFileNotFoundException), "Extensions with no transfer file should be detected.")]
        public void GetTransferFileExtensionForNoTransferFile() => Extensions.GetTransferFileExtension(new[] { ".ili" });

        [TestMethod]
        [ExpectedException(typeof(TransferFileNotFoundException), "Empty extensions should be detected.")]
        public void GetTransferFileExtensionForEmpty() => Extensions.GetTransferFileExtension(Enumerable.Empty<string>());

        [TestMethod]
        [ExpectedException(typeof(MultipleTransferFileFoundException), "Multiple transfer file extensions of the same type should be detected.")]
        public void GetTransferFileExtensionForMultiple() => Extensions.GetTransferFileExtension(new[] { ".itf", ".itf" });

        [TestMethod]
        [ExpectedException(typeof(UnknownExtensionException), "An unknown transfer file extension should be rejected.")]
        public void GetTransferFileExtensionForUnknownExtension() => Extensions.GetTransferFileExtension(new[] { ".sh" });

        [TestMethod]
        [ExpectedException(typeof(UnknownExtensionException), "GeoPackage (.gpkg) should be rejected if gpkg support is disabled.")]
        public void GetTransferFileExtensionForGpkgWithoutGpkgEnabled()
        {
            Environment.SetEnvironmentVariable("ENABLE_GPKG_VALIDATION", "false");
            Extensions.GetTransferFileExtension(new[] { ".gpkg" });
        }
    }
}
