﻿using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace ILICheck.Web
{
    [TestClass]
    public class PhysicalFileProviderTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void PhysicalFileProviderForNullEnvKey() =>
            Assert.ThrowsException<ArgumentNullException>(() => new PhysicalFileProvider(CreateConfiguration(), null));

        [TestMethod]
        public void Initialize()
        {
            var id = new Guid("59da9c46-dad0-41c7-b58f-8fa356d2d9fc");
            var physicalFileProvider = new PhysicalFileProvider(CreateConfiguration(), "ILICHECK_UPLOADS_DIR");
            physicalFileProvider.Initialize(id);

            var expectedHomeDirectory = Path.Combine(TestContext.DeploymentDirectory, id.ToString());
            var expectedHomeDirectoryPathFormat = "$ILICHECK_UPLOADS_DIR/59da9c46-dad0-41c7-b58f-8fa356d2d9fc/";

            Assert.AreEqual(expectedHomeDirectory, physicalFileProvider.HomeDirectory.FullName);
            Assert.AreEqual(expectedHomeDirectoryPathFormat, physicalFileProvider.HomeDirectoryPathFormat);
        }

        [TestMethod]
        public void InitializeForInvalid()
        {
            var physicalFileProvider = new PhysicalFileProvider(CreateConfiguration(), "ILICHECK_UPLOADS_DIR");

            Assert.ThrowsException<ArgumentException>(() => physicalFileProvider.Initialize(Guid.Empty));
        }

        private IConfiguration CreateConfiguration() =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ILICHECK_UPLOADS_DIR", TestContext.DeploymentDirectory },
            }).Build();
    }
}
