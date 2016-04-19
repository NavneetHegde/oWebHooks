using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using OWebHooks;
using System.ComponentModel;
using System.Net.Http;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpConfiguration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public static void InitializeAuthenticatedWebHooksSender(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            WebHooksConfig.Initialize(config);

            ILogger logger = config.DependencyResolver.GetLogger();
            HttpClient client = config.DependencyResolver.GetService<HttpClient>();

            // setting the custom sender
            IWebHookSender sender = new AuthorizedWebHookSender(logger, client);
            CustomServices.SetSender(sender);
        }

    }
}
