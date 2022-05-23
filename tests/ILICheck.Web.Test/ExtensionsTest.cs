using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ILICheck.Web
{
    [TestClass]
    public class ExtensionsTest
    {
        [TestMethod]
        public void Join()
        {
            Assert.AreEqual(string.Empty, Enumerable.Empty<string>().Join(","));
            Assert.AreEqual("1/2", new[] { "1", "2" }.Join("/"));
            Assert.AreEqual("BLUECALENDAR;HOPPINGNIGHT;NIGHTMAESTRO", new[] { "BLUECALENDAR", "HOPPINGNIGHT", "NIGHTMAESTRO" }.Join(";"));
            Assert.AreEqual(".abc, .de, .test", new[] { ".abc", ".de", ".test" }.Join(", "));
            Assert.AreEqual("foo bar", new[] { "foo", "bar" }.Join(" "));
            Assert.AreEqual("foo,bar", new[] { "foo", (string)null, "", " ", "bar" }.Join(","));

            Assert.ThrowsException<ArgumentNullException>(() => ((IEnumerable<string>)null).Join(","), "The string collection should not be null.");
            Assert.ThrowsException<InvalidOperationException>(() => new[] { "foo", "bar" }.Join(null), "Null seperator should be rejected.");
            Assert.ThrowsException<InvalidOperationException>(() => new[] { "foo", "bar" }.Join(""), "Empty seperator should be rejected.");
        }

        [TestMethod]
        public void CleanupGpkgModelNames()
        {
            var configuration = CreateConfiguration(blacklistedGpkgModels: "VIOLENTGLEE;CALENDARSEAGULL");
            var input = new[]
            {
                "ANGRYCHEF IRATENIGHT { REDTOTE Product Data} MAESTROTRAWL{ PEEVEDSCAN} VIOLENTGLEE {INTERLIS }",
                "SLEEPYGOPHER",
                "CALENDARSEAGULL {REV4}",
                " AUTOMAESTRO {VIOLETCALENDAR } ",
                "VIOLENTTOTE VIOLENTTOTE",
            };

            var expected = new[]
            {
                "ANGRYCHEF",
                "IRATENIGHT",
                "MAESTROTRAWL",
                "SLEEPYGOPHER",
                "AUTOMAESTRO",
                "VIOLENTTOTE",
            };

            CollectionAssert.AreEqual(expected, input.CleanupGpkgModelNames(configuration).ToList());
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

            Assert.AreEqual(".xtf", configuration.GetTransferFileExtension(new[] { ".gpkg", ".xml", ".ILI", ".xtf", ".itf" }));
            Assert.AreEqual(".ITF", configuration.GetTransferFileExtension(new[] { ".GPKG", ".xml", ".ili", ".ITF" }));
            Assert.AreEqual(".xml", configuration.GetTransferFileExtension(new[] { ".gpkg", ".xml", ".ili" }));
            Assert.AreEqual(".XML", configuration.GetTransferFileExtension(new[] { ".gpkg", ".XML" }));
            Assert.AreEqual(".itf", configuration.GetTransferFileExtension(new[] { ".gpkg", ".itf" }));
            Assert.AreEqual(".gpkg", configuration.GetTransferFileExtension(new[] { ".gpkg" }));
            Assert.AreEqual(".GPkg", configuration.GetTransferFileExtension(new[] { ".GPkg" }));
        }

        [TestMethod]
        public void GetTransferFileExtensionForMultipleModelAndCatalogueItems()
        {
            var configuration = CreateConfiguration(enableGpkgValidation: false);

            Assert.AreEqual(".itf", configuration.GetTransferFileExtension(new[] { ".itf", ".ili", ".ILI" }));
            Assert.AreEqual(".xtf", configuration.GetTransferFileExtension(new[] { ".xtf", ".xml", ".xml", ".ili", ".ili" }));
        }

        [TestMethod]
        public void GetTransferFileExtensionForInvalid()
        {
            var configuration = CreateConfiguration(enableGpkgValidation: false);

            Assert.ThrowsException<ArgumentNullException>(() => configuration.GetTransferFileExtension(null), "Null argument should be rejected.");
            Assert.ThrowsException<TransferFileNotFoundException>(() => configuration.GetTransferFileExtension(new[] { ".ili" }), "Extensions with no transfer file should be detected.");
            Assert.ThrowsException<TransferFileNotFoundException>(() => configuration.GetTransferFileExtension(Enumerable.Empty<string>()), "Empty extensions should be detected.");
            Assert.ThrowsException<MultipleTransferFileFoundException>(() => configuration.GetTransferFileExtension(new[] { ".itf", ".itf" }), "Multiple transfer file extensions of the same type should be detected.");
            Assert.ThrowsException<UnknownExtensionException>(() => configuration.GetTransferFileExtension(new[] { ".sh" }), "An unknown transfer file extension should be rejected.");
            Assert.ThrowsException<UnknownExtensionException>(() => configuration.GetTransferFileExtension(new[] { ".gpkg" }), "GeoPackage (.gpkg) should be rejected if gpkg support is disabled.");
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownExtensionException), "GeoPackage (.gpkg) should be rejected if gpkg support is disabled.")]
        public void GetTransferFileExtensionForGpkgWithoutGpkgEnabled() =>
            CreateConfiguration(enableGpkgValidation: false).GetTransferFileExtension(new[] { ".gpkg" });

        [TestMethod]
        public void GetFilesToDeleteWithDeleteTransferFilesEnabled()
        {
            var configuration = CreateConfiguration(deleteTransferFiles: true);
            var transferFile = @"\\IRATESOURCE\WAFFLETOTE.xtf";
            var files = new[]
            {
                @"C:\Temp\IRONAUTO.log",
                @"\\UNITEDWATCH\WRONGFARM.xtf",
                @"\\IRATESOURCE\WAFFLETOTE.xtf",
                @"/home/SPATULASCAN/LOUDFIRE.zip",
                @"IRATENET.ili",
                @"C:\Users\ANGRYSET WRONGBAGEL\REDBAGEL.gpkg",
                @"C:\Data\UNITEDTOLL\YARDGLEE-HX.xml",
                @"VIOLETDEITY.itf",
            };

            var expected = new[]
            {
                @"\\IRATESOURCE\WAFFLETOTE.xtf",
                @"/home/SPATULASCAN/LOUDFIRE.zip",
                @"IRATENET.ili",
                @"C:\Users\ANGRYSET WRONGBAGEL\REDBAGEL.gpkg",
                @"C:\Data\UNITEDTOLL\YARDGLEE-HX.xml",
                @"VIOLETDEITY.itf",
            };

            CollectionAssert.AreEqual(expected, configuration.GetFilesToDelete(files, transferFile).ToList());
        }

        [TestMethod]
        public void GetFilesToDeleteWithDeleteTransferFilesDisabled()
        {
            var configuration = CreateConfiguration(deleteTransferFiles: false);
            var transferFile = @"\\IRATESOURCE\WAFFLETOTE.xtf";
            var files = new[]
            {
                @"C:\Temp\IRONAUTO.log",
                @"\\UNITEDWATCH\WRONGFARM.xtf",
                @"\\IRATESOURCE\WAFFLETOTE.xtf",
                @"/home/SPATULASCAN/LOUDFIRE.zip",
                @"IRATENET.ili",
                @"C:\Users\ANGRYSET WRONGBAGEL\REDBAGEL.gpkg",
                @"C:\Data\UNITEDTOLL\YARDGLEE-HX.xml",
                @"VIOLETDEITY.itf",
            };

            var expected = Enumerable.Empty<string>().ToList();

            CollectionAssert.AreEqual(expected, configuration.GetFilesToDelete(files, transferFile).ToList());
        }

        [TestMethod]
        public void GetSanitizedFileExtension()
        {
            var configuration = CreateConfiguration(enableGpkgValidation: false);

            Assert.AreEqual(".xml", configuration.GetSanitizedFileExtension("VIOLETNIGHT.xml"));
            Assert.AreEqual(".zip", configuration.GetSanitizedFileExtension("SLIMYSOURCE.zip"));
            Assert.AreEqual(".xtf", configuration.GetSanitizedFileExtension("TRAWLMASTER.xtf"));
            Assert.AreEqual(".xml", configuration.GetSanitizedFileExtension("TRAWLMASTER.FARMARTIST.XML"));
            Assert.AreEqual(".zip", configuration.GetSanitizedFileExtension("GOPHERFELONY SCANWAFFLE .zIP"));
            Assert.ThrowsException<InvalidOperationException>(() => configuration.GetSanitizedFileExtension("SLICKERTRAWL.gpkg"));

            configuration = CreateConfiguration(enableGpkgValidation: true);

            Assert.AreEqual(".gpkg", configuration.GetSanitizedFileExtension("SLICKERTRAWL.gpkg"));

            // Not supported/invalid file extensions
            Assert.ThrowsException<InvalidOperationException>(() => configuration.GetSanitizedFileExtension("TRAWLBOUNCE.ini"));
            Assert.ThrowsException<InvalidOperationException>(() => configuration.GetSanitizedFileExtension("BIZARREPENGUIN.sh"));
            Assert.ThrowsException<InvalidOperationException>(() => configuration.GetSanitizedFileExtension("LATENTNET-HX.exe"));
            Assert.ThrowsException<InvalidOperationException>(() => configuration.GetSanitizedFileExtension("IRATEMONKEY.cmd"));
        }

        [TestMethod]
        public void GetLogFileForLog()
        {
            var fileProviderMock = new Mock<IFileProvider>();
            fileProviderMock.Setup(x => x.GetFiles()).Returns(new[] { "example.test", "example.log", "example.xtf", "example_LoG.lOg" });
            Assert.AreEqual("example_LoG.lOg", fileProviderMock.Object.GetLogFile(LogType.Log));
        }

        [TestMethod]
        public void GetLogFileForXtf()
        {
            var fileProviderMock = new Mock<IFileProvider>();
            fileProviderMock.Setup(x => x.GetFiles()).Returns(new[] { "example.test", "example.log", "example.xtf", "example_log.xTf" });
            Assert.AreEqual("example_log.xTf", fileProviderMock.Object.GetLogFile(LogType.Xtf));
        }

        private static IConfiguration CreateConfiguration(bool enableGpkgValidation = false, bool deleteTransferFiles = false, string blacklistedGpkgModels = "", string uploadsRootDir = "") =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ENABLE_GPKG_VALIDATION", enableGpkgValidation.ToString() },
                { "DELETE_TRANSFER_FILES", deleteTransferFiles.ToString() },
                { "Validation:BlacklistedGpkgModels", blacklistedGpkgModels },
                { "ILICHECK_UPLOADS_DIR", uploadsRootDir },
            }).Build();
    }
}
