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

        [TestMethod]
        public void GetSaveFileExtensionForFileName()
        {
            // Supported file extensions
            Assert.AreEqual(".xml", "VIOLETNIGHT.xml".GetSaveFileExtensionForFileName());
            Assert.AreEqual(".zip", "SLIMYSOURCE.zip".GetSaveFileExtensionForFileName());
            Assert.AreEqual(".xtf", "TRAWLMASTER.xtf".GetSaveFileExtensionForFileName());
            Assert.AreEqual(".xml", "TRAWLMASTER.FARMARTIST.XML".GetSaveFileExtensionForFileName());
            Assert.AreEqual(".zip", "GOPHERFELONY SCANWAFFLE .zIP".GetSaveFileExtensionForFileName());

            // Not supported file extensions
            Assert.IsNull("TRAWLBOUNCE.ini".GetSaveFileExtensionForFileName());
            Assert.IsNull("BIZARREPENGUIN.sh".GetSaveFileExtensionForFileName());
            Assert.IsNull("LATENTNET-HX.exe".GetSaveFileExtensionForFileName());
            Assert.IsNull("IRATEMONKEY.cmd".GetSaveFileExtensionForFileName());
        }
    }
}
