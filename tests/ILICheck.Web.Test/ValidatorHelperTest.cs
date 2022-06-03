using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using static ILICheck.Web.ValidatorHelper;

namespace ILICheck.Web
{
    [TestClass]
    public class ValidatorHelperTest
    {
        [TestMethod]
        public void GetAcceptedFileExtensionsForUserUploadsWithoutGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".zip" };
            var configuration = CreateConfiguration(enableGpkgValidation: false);
            CollectionAssert.AreEqual(expected, GetAcceptedFileExtensionsForUserUploads(configuration).ToList());
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForUserUploadsWithGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".gpkg", ".zip" };
            var configuration = CreateConfiguration(enableGpkgValidation: true);
            CollectionAssert.AreEqual(expected, GetAcceptedFileExtensionsForUserUploads(configuration).ToList());
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForZipContentWithoutGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".ili" };
            var configuration = CreateConfiguration(enableGpkgValidation: false);
            CollectionAssert.AreEqual(expected, GetAcceptedFileExtensionsForZipContent(configuration).ToList());
        }

        [TestMethod]
        public void GetAcceptedFileExtensionsForZipContentWithGpkgEnabled()
        {
            var expected = new[] { ".xtf", ".itf", ".xml", ".gpkg", ".ili" };
            var configuration = CreateConfiguration(enableGpkgValidation: true);
            CollectionAssert.AreEqual(expected, GetAcceptedFileExtensionsForZipContent(configuration).ToList());
        }

        [TestMethod]
        public void GetIlivalidatorCommand()
        {
            AssertGetIlivalidatorCommand(
                "dada hopp monkey:latest sh ilivalidator --log /PEEVEDBAGEL/ANT_log.log --xtflog /PEEVEDBAGEL/ANT_log.xtf \"/PEEVEDBAGEL/ANT.XTF\"",
                "dada hopp monkey:latest sh",
                "/PEEVEDBAGEL/",
                "ANT.XTF",
                null);

            AssertGetIlivalidatorCommand(
                "ilivalidator --log foo/bar/SETNET_log.log --xtflog foo/bar/SETNET_log.xtf --models \"ANGRY;SQUIRREL\" \"foo/bar/SETNET.abc\"",
                null,
                "foo/bar",
                "SETNET.abc",
                "ANGRY;SQUIRREL");

            AssertGetIlivalidatorCommand(
                "ilivalidator --log ${SEA}/RED/WATCH_log.log --xtflog ${SEA}/RED/WATCH_log.xtf \"${SEA}/RED/WATCH.GPKG\"",
                string.Empty,
                "${SEA}/RED/",
                "WATCH.GPKG",
                string.Empty);
        }

        private static void AssertGetIlivalidatorCommand(string expected, string prefix, string homeDirectory, string transferFile, string models) =>
            Assert.AreEqual(expected, ValidatorHelper.GetIlivalidatorCommand(CreateConfiguration(commandPrefix: prefix), homeDirectory, transferFile, models));

        private static IConfiguration CreateConfiguration(bool enableGpkgValidation = false, string commandPrefix = "") =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ENABLE_GPKG_VALIDATION", enableGpkgValidation.ToString() },
                { "Validation:CommandPrefix", commandPrefix },
            }).Build();
    }
}
