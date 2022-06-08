using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace ILICheck.Web.Controllers
{
    [TestClass]
    public sealed class StatusControllerTest
    {
        private Mock<ILogger<StatusController>> loggerMock;
        private Mock<IValidatorService> validatorServiceMock;
        private Mock<ApiVersion> apiVersionMock;
        private StatusController controller;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger<StatusController>>();
            validatorServiceMock = new Mock<IValidatorService>(MockBehavior.Strict);
            apiVersionMock = new Mock<ApiVersion>(MockBehavior.Strict, 8, 77);

            controller = new StatusController(
                loggerMock.Object,
                validatorServiceMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock.VerifyAll();
            validatorServiceMock.VerifyAll();
            apiVersionMock.VerifyAll();

            controller.Dispose();
        }

        [TestMethod]
        public void GetStatus()
        {
            var jobId = new Guid("fadc5142-9043-4fdc-aebf-36c21e13f621");
            validatorServiceMock
                .Setup(x => x.GetJobStatusOrDefault(It.Is<string>(x => x.Equals(jobId.ToString(), StringComparison.Ordinal))))
                .Returns((Status.Processing, "WAFFLESPATULA GREENNIGHT"));

            var response = controller.GetStatus(apiVersionMock.Object, jobId) as OkObjectResult;

            Assert.IsInstanceOfType(response, typeof(OkObjectResult));
            Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
            Assert.AreEqual("{ jobId = fadc5142-9043-4fdc-aebf-36c21e13f621, status = Processing, statusMessage = WAFFLESPATULA GREENNIGHT, logUrl = /api/v8/download?jobId=fadc5142-9043-4fdc-aebf-36c21e13f621&logType=log, xtfLogUrl = /api/v8/download?jobId=fadc5142-9043-4fdc-aebf-36c21e13f621&logType=xtf }", response.Value.ToString());
        }

        [TestMethod]
        public void GetStatusForInvalid()
        {
            validatorServiceMock
                .Setup(x => x.GetJobStatusOrDefault(It.Is<string>(x => x.Equals(Guid.Empty.ToString(), StringComparison.Ordinal))))
                .Returns((default, default));

            var response = controller.GetStatus(apiVersionMock.Object, default) as ObjectResult;

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            Assert.AreEqual(StatusCodes.Status404NotFound, response.StatusCode);
            Assert.AreEqual("No job information available for job id <00000000-0000-0000-0000-000000000000>", ((ProblemDetails)response.Value).Detail);
        }
    }
}
