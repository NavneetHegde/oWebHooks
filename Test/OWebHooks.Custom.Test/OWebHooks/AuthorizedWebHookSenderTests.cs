using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace OWebHooks
{
    [TestClass]
    public class AuthorizedWebHookSenderTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly HttpMessageHandlerMock _handlerMock;
        private AuthorizedWebHookSender _sender;
        private readonly Mock<ILogger> _loggerMock;


        public AuthorizedWebHookSenderTests()
        {
            _handlerMock = new HttpMessageHandlerMock();
            _httpClient = new HttpClient(_handlerMock);
            _loggerMock = new Mock<ILogger>();
        }

        [TestMethod]
        public void SendWebHook_VerifyAllAuth()
        {
            //Arrange
            var workItems = GetWorkItems();
            _handlerMock.Handler = GetHttpRespone();

            _sender = new AuthorizedWebHookSender(_loggerMock.Object, _httpClient);

            //Act
            var actual = _sender.SendWebHookWorkItemsAsync(workItems);

            //Assert
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void SendWebHook_ResponseHasErrir()
        {
            //Arrange
            var workItems = GetWorkItems();
            _handlerMock.Handler = GetHttpErrorRespone();

            _sender = new AuthorizedWebHookSender(_loggerMock.Object, _httpClient);

            //Act
            var actual = _sender.SendWebHookWorkItemsAsync(workItems);

            //Assert
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void SendWebHook_ContentOfResponseIsNull()
        {
            //Arrange
            var workItems = GetWorkItems();
            _handlerMock.Handler = GetHttpNullContentRespone();

            _sender = new AuthorizedWebHookSender(_loggerMock.Object, _httpClient);

            //Act
            var actual = _sender.SendWebHookWorkItemsAsync(workItems);

            //Assert
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void SendWebHook_ExceptionThrown()
        {
            //Arrange
            var workItems = GetWorkItems();
            _handlerMock.Handler = null;

            _sender = new AuthorizedWebHookSender(_loggerMock.Object, _httpClient);

            //Act
            var actual = _sender.SendWebHookWorkItemsAsync(workItems);

            //Assert
            Assert.IsNotNull(actual);
        }

        #region private methods

        private Func<HttpRequestMessage, Task<HttpResponseMessage>> GetHttpRespone()
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("access_token", "sasdasdasdasdadadas");

            return (req) =>
            {
                HttpResponseMessage response = new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            };
        }

        private Func<HttpRequestMessage, Task<HttpResponseMessage>> GetHttpNullContentRespone()
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("access_token", "sasdasdasdasdadadas");

            return (req) =>
            {
                HttpResponseMessage response = new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = null
                };
                return Task.FromResult(response);
            };
        }

        private Func<HttpRequestMessage, Task<HttpResponseMessage>> GetHttpErrorRespone()
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("access_token", "sasdasdasdasdadadas");

            return (req) =>
            {
                HttpResponseMessage response = new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            };
        }

        private IEnumerable<WebHookWorkItem> GetWorkItems()
        {
            NotificationDictionary notification = new NotificationDictionary("Update", data: null);

            //OAuth
            WebHook webHookOAuth = CreateWebHook("update");
            webHookOAuth.Properties.Add("auth_type", "oauth");
            webHookOAuth.Properties.Add("username", "username");
            webHookOAuth.Properties.Add("password", "password");
            webHookOAuth.Properties.Add("token_url", "authtokenurl");
            webHookOAuth.Properties.Add("grant_type", "password");

            //Basic Auth
            WebHook webHookBasicAuth = CreateWebHook("update");
            webHookBasicAuth.Properties.Add("auth_type", "basic");
            webHookBasicAuth.Properties.Add("username", "username");
            webHookBasicAuth.Properties.Add("password", "password");

            // No Auth
            WebHook webHookNoAuth = CreateWebHook("update");

            //oAuth missing the properties
            WebHook webHookOAuthMissingProp = CreateWebHook("update");
            webHookOAuthMissingProp.Properties.Add("auth_type", "oauth");
            webHookOAuthMissingProp.Properties.Add("username", "username");
            webHookOAuthMissingProp.Properties.Add("password", "password");
            webHookOAuthMissingProp.Properties.Add("grant_type", "password");

            return new List<WebHookWorkItem>
           {
               new WebHookWorkItem(webHookOAuth, new [] { notification }) {  Id="12345" },
               new WebHookWorkItem(webHookBasicAuth, new [] { notification }) {  Id="12345" },
               new WebHookWorkItem(webHookNoAuth, new [] { notification }) {  Id="12345" },
               new WebHookWorkItem(webHookOAuthMissingProp, new [] { notification }) {  Id="12345" }
           };
        }

        private WebHook CreateWebHook(params string[] filters)
        {
            WebHook hook = new WebHook
            {
                Id = "1234",
                Description = "Test Hook",
                Secret = "123456789012345678901234567890123456789012345678",
                WebHookUri = new Uri("http://localhost/hook"),
            };
            hook.Headers.Add("h1", "hv1");
            hook.Properties.Add("p1", "pv1");

            foreach (string filter in filters)
            {
                hook.Filters.Add(filter);
            }
            return hook;
        }

        #endregion

        public void Dispose()
        {
            if (_sender != null)
            {
                _sender.Dispose();
            }
            if (_handlerMock != null)
            {
                _handlerMock.Dispose();
            }
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }

}
