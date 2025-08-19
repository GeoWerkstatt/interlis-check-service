using Geowerkstatt.Ilicop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace Geowerkstatt.Ilicop.Web.Controllers
{
    [TestClass]
    public sealed class DownloadControllerTest
    {
        private readonly Guid jobId = Guid.Parse("4dcc790d-541a-4d34-bf91-bc9c59360fee");

        private Mock<ILogger<DownloadController>> loggerMock;
        private Mock<IFileProvider> fileProviderMock;
        private DownloadController controller;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger<DownloadController>>();
            fileProviderMock = new Mock<IFileProvider>(MockBehavior.Strict);

            controller = new DownloadController(loggerMock.Object, fileProviderMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock.VerifyAll();
            fileProviderMock.VerifyAll();

            controller.Dispose();
        }

        [TestMethod]
        public void Download()
        {
            fileProviderMock.Setup(x => x.Initialize(jobId));
            fileProviderMock.Setup(x => x.GetFiles()).Returns(new[] { "OCEANSTEED_log.xtf", "DARKFOOT_log.log", "HAPPYPOINT_log.geojson" });
            fileProviderMock.Setup(x => x.OpenText(It.IsAny<string>())).Returns(StreamReader.Null);

            void AssertContentType(LogType logType, string contentType)
            {
                var result = controller.Download(jobId, logType) as FileStreamResult;
                Assert.AreEqual(result.ContentType, contentType);
            }

            AssertContentType(LogType.Log, "text/plain");
            AssertContentType(LogType.Xtf, "text/xml; charset=utf-8");
            AssertContentType(LogType.GeoJson, "application/geo+json");
        }
    }
}
