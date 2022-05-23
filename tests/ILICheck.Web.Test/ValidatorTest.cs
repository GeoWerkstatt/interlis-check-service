﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ILICheck.Web
{
    [TestClass]
    public class ValidatorTest
    {
        private Mock<ILogger<Validator>> loggerMock;
        private Mock<PhysicalFileProvider> fileProviderMock;
        private Mock<Validator> validatorMock;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger<Validator>>();
            fileProviderMock = new Mock<PhysicalFileProvider>(MockBehavior.Strict, TestContext.DeploymentDirectory);
            validatorMock = new Mock<Validator>(MockBehavior.Strict, loggerMock.Object, CreateConfiguration(), fileProviderMock.Object);

            validatorMock.SetupGet(x => x.Id).Returns("testdata");
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock.VerifyAll();
            fileProviderMock.VerifyAll();
            validatorMock.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Null argument should be rejected.")]
        public async Task ValidateXmlAsyncForNull()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns((string)null);
            await validatorMock.Object.ValidateXmlAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Empty argument should be rejected.")]
        public async Task ValidateXmlAsyncForEmpty()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns(string.Empty);
            await validatorMock.Object.ValidateXmlAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException), "Corrupt or invalid transfer file should be rejected.")]
        public async Task ValidateXmlAsyncForInvalid()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns("invalid.xtf");
            await validatorMock.Object.ValidateXmlAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException), "Not existing file path should be detected.")]
        public async Task ValidateXmlAsyncForFileNotFound()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns("unavailable.xtf");
            await validatorMock.Object.ValidateXmlAsync();
        }

        [TestMethod]
        public async Task ValidateXmlAsync()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns("example.xtf");
            await validatorMock.Object.ValidateXmlAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Null argument should be rejected.")]
        public async Task ReadGpkgModelNamesAsyncForNull()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns((string)null);
            await validatorMock.Object.ReadGpkgModelNamesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Empty argument should be rejected.")]
        public async Task ReadGpkgModelNamesAsyncForEmpty()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns(string.Empty);
            await validatorMock.Object.ReadGpkgModelNamesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(GeoPackageException), "Corrupt or invalid GeoPackage should be rejected.")]
        public async Task ReadGpkgModelNamesAsyncForInvalid()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns("invalid.gpkg");
            await validatorMock.Object.ReadGpkgModelNamesAsync();
        }

        [TestMethod]
        public async Task ReadGpkgModelNamesAsync()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns("example.gpkg");
            var expected = "Wildruhezonen_Codelisten_V2_1;Wildruhezonen_LV03_V2_1;Wildruhezonen_LV95_V2_1;LOUDTRINITY";

            Assert.AreEqual(expected, await validatorMock.Object.ReadGpkgModelNamesAsync());
        }

        [TestMethod]
        public async Task CleanUploadDirectoryAsync()
        {
            validatorMock.SetupGet(x => x.TransferFile).Returns("example.xtf");
            fileProviderMock.Setup(x => x.GetFiles()).Returns(new[] { "example.ili", "example.xtf", "example_log.xtf" });
            fileProviderMock.Setup(x => x.DeleteFileAsync(It.Is<string>(x => x == "example.xtf"))).Returns(Task.FromResult(0));
            fileProviderMock.Setup(x => x.DeleteFileAsync(It.Is<string>(x => x == "example.ili"))).Returns(Task.FromResult(0));

            await validatorMock.Object.CleanUploadDirectoryAsync();
        }

        private static IConfiguration CreateConfiguration() =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "DELETE_TRANSFER_FILES", "TRUE" },
                { "Validation:BlacklistedGpkgModels", "PEEVEDSCAN;WRONGPENGUIN;SPORKDEITY;CHBaseEx_MapCatalogue_V1;CHBaseEx_WaterNet_V1;CHBaseEx_Sewage_V1;CHAdminCodes_V1;AdministrativeUnits_V1;AdministrativeUnitsCH_V1;WithOneState_V1;WithLatestModification_V1;WithModificationObjects_V1;GraphicCHLV03_V1;GraphicCHLV95_V1;NonVector_Base_V2;NonVector_Base_V3;NonVector_Base_LV03_V3_1;NonVector_Base_LV95_V3_1;GeometryCHLV03_V1;GeometryCHLV95_V1;InternationalCodes_V1;Localisation_V1;LocalisationCH_V1;Dictionaries_V1;DictionariesCH_V1;CatalogueObjects_V1;CatalogueObjectTrees_V1;AbstractSymbology;CodeISO;CoordSys;GM03_2_1Comprehensive;GM03_2_1Core;GM03_2Comprehensive;GM03_2Core;GM03Comprehensive;GM03Core;IliRepository09;IliSite09;IlisMeta07;IliVErrors;INTERLIS_ext;RoadsExdm2ben;RoadsExdm2ben_10;RoadsExgm2ien;RoadsExgm2ien_10;StandardSymbology;StandardSymbology;Time;Units" },
            }).Build();
    }
}
