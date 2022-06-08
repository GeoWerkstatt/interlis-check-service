using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ILICheck.Web.Controllers
{
    [TestClass]
    public sealed class UploadControllerTest
    {
        private Mock<ILogger<UploadController>> loggerMock;
        private Mock<IHttpContextAccessor> httpContextAccessorMock;
        private Mock<IValidator> validatorMock;
        private Mock<PhysicalFileProvider> fileProviderMock;
        private Mock<ApiVersion> apiVersionMock;
        private Mock<IFormFile> formFileMock;
        private Mock<IValidatorService> validatorServiceMock;
        private UploadController controller;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger<UploadController>>();
            httpContextAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            validatorMock = new Mock<IValidator>(MockBehavior.Strict);
            fileProviderMock = new Mock<PhysicalFileProvider>(MockBehavior.Strict, CreateConfiguration(), "ILICHECK_UPLOADS_DIR");
            validatorServiceMock = new Mock<IValidatorService>(MockBehavior.Strict);
            formFileMock = new Mock<IFormFile>(MockBehavior.Strict);
            apiVersionMock = new Mock<ApiVersion>(MockBehavior.Strict, 9, 88);

            validatorMock.SetupGet(x => x.Id).Returns("testdata");

            controller = new UploadController(
                loggerMock.Object,
                CreateConfiguration(),
                httpContextAccessorMock.Object,
                validatorMock.Object,
                fileProviderMock.Object,
                validatorServiceMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock.VerifyAll();
            httpContextAccessorMock.VerifyAll();
            validatorMock.VerifyAll();
            fileProviderMock.VerifyAll();
            formFileMock.VerifyAll();
            validatorServiceMock.VerifyAll();
            apiVersionMock.VerifyAll();

            controller.Dispose();
        }

        [TestMethod]
        public async Task UploadAsync()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentLength = 1234;
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            formFileMock.SetupGet(x => x.FileName).Returns("BIZARRESCAN.xtf");
            formFileMock.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
            validatorServiceMock.Setup(x => x.EnqueueJobAsync(
                It.Is<string>(x => x.Equals("testdata", StringComparison.Ordinal)),
                It.IsAny<Func<CancellationToken, Task>>())).Returns(Task.FromResult(0));

            var response = await controller.UploadAsync(apiVersionMock.Object, formFileMock.Object) as CreatedResult;

            Assert.IsInstanceOfType(response, typeof(CreatedResult));
            Assert.AreEqual(201, response.StatusCode);
            Assert.AreEqual("/api/v9/status/testdata", response.Location);
            Assert.AreEqual("{ jobId = testdata, statusUrl = /api/v9/status/testdata }", response.Value.ToString());
        }

        [TestMethod]
        public async Task UploadAsyncForNull()
        {
            var response = await controller.UploadAsync(apiVersionMock.Object, null) as ObjectResult;

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            Assert.AreEqual(400, response.StatusCode);
            Assert.AreEqual("Form data <file> cannot be empty.", ((ProblemDetails)response.Value).Detail);
        }

        [TestMethod]
        public async Task UploadAsyncForInvalid()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentLength = 1234;
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            formFileMock.SetupGet(x => x.FileName).Returns("SPATULASET.cmd");

            var response = await controller.UploadAsync(apiVersionMock.Object, formFileMock.Object) as ObjectResult;

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            Assert.AreEqual(400, response.StatusCode);
            Assert.AreEqual("Transfer file extension <.cmd> is an unknown file extension.", ((ProblemDetails)response.Value).Detail);
        }

        private IConfiguration CreateConfiguration(bool enableGpkgValidation = false) =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ILICHECK_UPLOADS_DIR", TestContext.DeploymentDirectory },
                { "ENABLE_GPKG_VALIDATION", enableGpkgValidation.ToString() },
            }).Build();
    }
}
