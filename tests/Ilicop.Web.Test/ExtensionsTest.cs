using Geowerkstatt.Ilicop.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Geowerkstatt.Ilicop.Web
{
    [TestClass]
    public class ExtensionsTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void JoinNonEmpty()
        {
            Assert.AreEqual(string.Empty, Enumerable.Empty<string>().JoinNonEmpty(","));
            Assert.AreEqual("1/2", new[] { "1", "2" }.JoinNonEmpty("/"));
            Assert.AreEqual("BLUECALENDAR;HOPPINGNIGHT;NIGHTMAESTRO", new[] { "BLUECALENDAR", "HOPPINGNIGHT", "NIGHTMAESTRO" }.JoinNonEmpty(";"));
            Assert.AreEqual(".abc, .de, .test", new[] { ".abc", ".de", ".test" }.JoinNonEmpty(", "));
            Assert.AreEqual("foo bar", new[] { "foo", "bar" }.JoinNonEmpty(" "));
            Assert.AreEqual("foo,bar", new[] { "foo", (string)null, "", " ", "bar" }.JoinNonEmpty(","));

            Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<string>)null).JoinNonEmpty(","), "The string collection should not be null.");
            Assert.ThrowsExactly<InvalidOperationException>(() => new[] { "foo", "bar" }.JoinNonEmpty(null), "Null seperator should be rejected.");
            Assert.ThrowsExactly<InvalidOperationException>(() => new[] { "foo", "bar" }.JoinNonEmpty(""), "Empty seperator should be rejected.");
        }

        [TestMethod]
        public void GetTransferFileExtensionWithoutGpkgEnabled()
        {
            var configuration = CreateConfiguration(enableGpkgValidation: false);

            Assert.AreEqual(".XTF", new[] { ".xml", ".ili", ".XTF", ".itf" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".itf", new[] { ".xml", ".ili", ".itf" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".XML", new[] { ".XML", ".ili" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".xml", new[] { ".xml" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".itf", new[] { ".itf" }.GetTransferFileExtension(configuration));
        }

        [TestMethod]
        public void GetTransferFileExtensionWithGpkgEnabled()
        {
            var configuration = CreateConfiguration(enableGpkgValidation: true);

            Assert.AreEqual(".xtf", new[] { ".gpkg", ".xml", ".ILI", ".xtf", ".itf" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".ITF", new[] { ".GPKG", ".xml", ".ili", ".ITF" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".xml", new[] { ".gpkg", ".xml", ".ili" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".XML", new[] { ".gpkg", ".XML" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".itf", new[] { ".gpkg", ".itf" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".gpkg", new[] { ".gpkg" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".GPkg", new[] { ".GPkg" }.GetTransferFileExtension(configuration));
        }

        [TestMethod]
        public void GetTransferFileExtensionForMultipleModelAndCatalogueItems()
        {
            var configuration = CreateConfiguration(enableGpkgValidation: false);

            Assert.AreEqual(".itf", new[] { ".itf", ".ili", ".ILI" }.GetTransferFileExtension(configuration));
            Assert.AreEqual(".xtf", new[] { ".xtf", ".xml", ".xml", ".ili", ".ili" }.GetTransferFileExtension(configuration));
        }

        [TestMethod]
        public void GetTransferFileExtensionForInvalid()
        {
            var configuration = CreateConfiguration(enableGpkgValidation: false);

            Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<string>)null).GetTransferFileExtension(configuration), "Null argument should be rejected.");
            Assert.ThrowsExactly<TransferFileNotFoundException>(() => new[] { ".ili" }.GetTransferFileExtension(configuration), "Extensions with no transfer file should be detected.");
            Assert.ThrowsExactly<TransferFileNotFoundException>(() => Enumerable.Empty<string>().GetTransferFileExtension(configuration), "Empty extensions should be detected.");
            Assert.ThrowsExactly<MultipleTransferFileFoundException>(() => new[] { ".itf", ".itf" }.GetTransferFileExtension(configuration), "Multiple transfer file extensions of the same type should be detected.");
            Assert.ThrowsExactly<UnknownExtensionException>(() => new[] { ".sh" }.GetTransferFileExtension(configuration), "An unknown transfer file extension should be rejected.");
            Assert.ThrowsExactly<UnknownExtensionException>(() => new[] { ".gpkg" }.GetTransferFileExtension(configuration), "GeoPackage (.gpkg) should be rejected if gpkg support is disabled.");
        }

        [TestMethod]
        public void GetTransferFileExtensionForGpkgWithoutGpkgEnabled() =>
            Assert.ThrowsExactly<UnknownExtensionException>(() => new[] { ".gpkg" }.GetTransferFileExtension(CreateConfiguration(enableGpkgValidation: false)));

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
        public void GetFilesToDeleteWithDeleteTransferFilesEnabled()
        {
            var configuration = CreateConfiguration(deleteTransferFiles: true);
            var transferFile = @"\\IRATESOURCE\WAFFLETOTE.xtf";
            var input = new[]
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

            CollectionAssert.AreEqual(expected, input.GetFilesToDelete(configuration, transferFile).ToList());
        }

        [TestMethod]
        public void GetFilesToDeleteWithDeleteTransferFilesDisabled()
        {
            var configuration = CreateConfiguration(deleteTransferFiles: false);
            var transferFile = @"\\IRATESOURCE\WAFFLETOTE.xtf";
            var input = new[]
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

            CollectionAssert.AreEqual(expected, input.GetFilesToDelete(configuration, transferFile).ToList());
        }

        [TestMethod]
        public void GetSanitizedFileExtension()
        {
            var acceptedFileExtensions = ValidatorHelper.GetAcceptedFileExtensionsForUserUploads(CreateConfiguration(enableGpkgValidation: false));

            Assert.AreEqual(".xml", "VIOLETNIGHT.xml".GetSanitizedFileExtension(acceptedFileExtensions));
            Assert.AreEqual(".zip", "SLIMYSOURCE.zip".GetSanitizedFileExtension(acceptedFileExtensions));
            Assert.AreEqual(".xtf", "TRAWLMASTER.xtf".GetSanitizedFileExtension(acceptedFileExtensions));
            Assert.AreEqual(".xml", "TRAWLMASTER.FARMARTIST.XML".GetSanitizedFileExtension(acceptedFileExtensions));
            Assert.AreEqual(".zip", "GOPHERFELONY SCANWAFFLE .zIP".GetSanitizedFileExtension(acceptedFileExtensions));
            Assert.ThrowsExactly<UnknownExtensionException>(() => "SLICKERTRAWL.gpkg".GetSanitizedFileExtension(acceptedFileExtensions));

            acceptedFileExtensions = ValidatorHelper.GetAcceptedFileExtensionsForUserUploads(CreateConfiguration(enableGpkgValidation: true));

            Assert.AreEqual(".gpkg", "SLICKERTRAWL.gpkg".GetSanitizedFileExtension(acceptedFileExtensions));

            // Not supported/invalid file extensions
            Assert.ThrowsExactly<UnknownExtensionException>(() => "TRAWLBOUNCE.ini".GetSanitizedFileExtension(acceptedFileExtensions));
            Assert.ThrowsExactly<UnknownExtensionException>(() => "BIZARREPENGUIN.sh".GetSanitizedFileExtension(acceptedFileExtensions));
            Assert.ThrowsExactly<UnknownExtensionException>(() => "LATENTNET-HX.exe".GetSanitizedFileExtension(acceptedFileExtensions));
            Assert.ThrowsExactly<UnknownExtensionException>(() => "IRATEMONKEY.cmd".GetSanitizedFileExtension(acceptedFileExtensions));
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

        private static IConfiguration CreateConfiguration(bool enableGpkgValidation = false, bool deleteTransferFiles = false, string blacklistedGpkgModels = "") =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ENABLE_GPKG_VALIDATION", enableGpkgValidation.ToString() },
                { "DELETE_TRANSFER_FILES", deleteTransferFiles.ToString() },
                { "Validation:BlacklistedGpkgModels", blacklistedGpkgModels },
            }).Build();
    }
}
