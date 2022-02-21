using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
    }
}
