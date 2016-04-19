using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Newtonsoft.Json.Linq;
using OWebHooks.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OWebHooks
{

    /// <summary>
    /// 
    /// </summary>
    public class AuthorizedWebHookSender : DataflowWebHookSender
    {

        private HttpClient _httpClient;
        private bool _disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClient"></param>
        public AuthorizedWebHookSender(ILogger logger, HttpClient httpClient) : base(logger)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workItems"></param>
        /// <returns></returns>
        public override async Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems)
        {
            Logger?.Info("SendWebHookWorkItemsAsync::Start");

            foreach (var item in workItems)
            {
                try
                {
                    var authType = GetAuthType(item);

                    Logger?.Info($"SendWebHookWorkItemsAsync::AuthType: {authType}");
                    switch (authType)
                    {
                        case AuthorizationType.Basic:
                            VerifyAndInjectBasicAuthHeader(item.WebHook);
                            break;
                        case AuthorizationType.oAuth:
                            await VerifyAndInjectOAuthHeader(item.WebHook);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger?.Error("AuthorizedWebHookSender::SendWebHookWorkItemsAsync:" + ex.ToString());
                }
            }

            await base.SendWebHookWorkItemsAsync(workItems);

            Logger?.Info("SendWebHookWorkItemsAsync::End");

        }

        /// <summary>
        /// Fetches the authorization type required for this webhook mentioned in the prperties
        /// </summary>
        /// <param name="webHookWorkItem">WebHookWorkItem</param>
        /// <returns>AuthorizationType</returns>
        private AuthorizationType GetAuthType(WebHookWorkItem webHookWorkItem)
        {
            Logger?.Info("GetAuthType::Start");
            string authType = GetWebHookPropertiesValue(webHookWorkItem.WebHook, CustomResources.Authorization_Type);

            Logger?.Info($"GetAuthType::Authtype: {authType} for WebHook Id: {webHookWorkItem.WebHook.Id}");
            switch (authType)
            {
                case "oauth":
                    return AuthorizationType.oAuth;
                case "basic":
                    return AuthorizationType.Basic;
                default:
                    return AuthorizationType.NoAuth;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webHook"></param>
        private void VerifyAndInjectBasicAuthHeader(WebHook webHook)
        {
            string authUsername = GetWebHookPropertiesValue(webHook, CustomResources.Authorization_UserName);
            string authPassword = GetWebHookPropertiesValue(webHook, CustomResources.Authorization_Password);

            string encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{authUsername}:{authPassword}"));

            Logger.Info($"VerifyAndInjectBasicAuthHeader:User:{encodedCredentials}");
            webHook.Headers.Add("Authorization", "basic " + encodedCredentials);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webHook"></param>
        /// <returns></returns>
        private async Task VerifyAndInjectOAuthHeader(WebHook webHook)
        {
            Logger?.Info("VerifyAndInjectOAuthHeader::Start");
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                HttpRequestMessage request = CreateWebHookAuthenticationRequest(webHook);

                response = await _httpClient.SendAsync(request);

                if (response == null)
                {
                    Logger.Error("Null response for webhook");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"VerifyAndInjectOAuthHeader::Error:" + ex?.ToString());
                return;
            }

            if (response.IsSuccessStatusCode == false)
            {
                Logger.Error($"Aothorization call failed with status code: {response.StatusCode} reason: {response.ReasonPhrase}");
                return;
            }

            if (response.Content == null)
            {
                Logger.Error($"No Content found  for webbhook id: {webHook.Id} : status code : {response.StatusCode}");
                return;
            }


            var result = await response.Content.ReadAsAsync<JObject>();
            string access_token = result?.GetValue("access_token")?.ToString();

            if (!string.IsNullOrWhiteSpace(access_token))
            {
                webHook.Headers.Add("Authorization", "bearer " + access_token);
            }
            Logger.Info("VerifyAndInjectOAuthHeader::End");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webHook"></param>
        /// <returns></returns>
        private HttpRequestMessage CreateWebHookAuthenticationRequest(WebHook webHook)
        {
            Logger.Info($"CreateWebHookAuthenticationRequest::Start");
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            // Get requet URI
            UriBuilder webHookUri = new UriBuilder(GetWebHookPropertiesValue(webHook, CustomResources.Authorization_Token_Url));

            // add headers to the request from webhook
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, webHookUri.Uri);

            // create body content
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            pairs.Add("username", GetWebHookPropertiesValue(webHook, CustomResources.Authorization_UserName));
            pairs.Add("password", GetWebHookPropertiesValue(webHook, CustomResources.Authorization_Password));
            pairs.Add("grant_type", GetWebHookPropertiesValue(webHook, CustomResources.Authorization_GrantType));

            FormUrlEncodedContent formContent = new FormUrlEncodedContent(pairs);
            request.Content = formContent;

            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");

            Logger.Info($"CreateWebHookAuthenticationRequest::End");

            return request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webHook"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetWebHookPropertiesValue(WebHook webHook, string key)
        {
            Logger.Info($"GetWebHookPropertiesValue::Key:{key}");
            if (webHook == null || string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return (webHook.Properties != null && webHook.Properties.ContainsKey(key))
                ? webHook.Properties[key].ToString()
                : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workItem"></param>
        /// <returns></returns>
        protected override Task OnWebHookSuccess(WebHookWorkItem workItem)
        {
            return base.OnWebHookSuccess(workItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        [ExcludeFromCodeCoverage]
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    try
                    {
                        if (_httpClient != null)
                        {
                            _httpClient.CancelPendingRequests();
                            _httpClient.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        ex = ex.GetBaseException();
                        string logMessage = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_CompletionFailure, ex.Message);
                        Logger.Error(logMessage);
                    }
                }
            }
            base.Dispose(disposing);
        }
    }

}
