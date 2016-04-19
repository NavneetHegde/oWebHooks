using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http;

namespace OWebHooks
{
    [TestClass]
    public class HttpConfigurationExtensionsTests
    {
        [TestMethod]
        public void Initialize_SetSender()
        {
            //Arrange
            HttpConfiguration config = new HttpConfiguration();
            ILogger logger = new TraceLogger();
            //Act
            config.InitializeAuthenticatedWebHooksSender();
            IWebHookSender actual = CustomServices.GetSender(logger);

            //Assert
            Assert.IsInstanceOfType(actual, typeof(AuthorizedWebHookSender));
        }
    }
}
