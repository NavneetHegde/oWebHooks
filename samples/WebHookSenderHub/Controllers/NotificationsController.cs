using Microsoft.AspNet.WebHooks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Routing;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Diagnostics;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;

namespace WebHookSenderHub.Controllers
{

    /// <summary>
    /// The <see cref="NotificationsController"/> allows the admin/user to trigger the notification based
    /// on event's with data.
    /// TODO : Authorize user based on login or send the IPrincipal to rquest.
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class NotificationsController : ApiController
    {
        private IWebHookFilterProvider _webHookFilterProvider;

        #region Init
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }

        #endregion

        public NotificationsController(IWebHookFilterProvider webHookFilterProvider)
        {
            _webHookFilterProvider = webHookFilterProvider;
        }

        /// <summary>
        /// This will send webhook to all registered events
        /// TODO :  Need to fetch event from Post data to fetch the Event to be raised
        /// </summary>
        /// <returns></returns>
        public async Task Post()
        {

            //send final notification
            await this.NotifyAllAsync("Update", new { P1 = "p1" });
        }

        [HttpGet]
        [Route("api/filters")]
        public async Task<Collection<WebHookFilter>> Filters()
        {
            var webHookFilter = await _webHookFilterProvider.GetFiltersAsync();
            return webHookFilter;
        }

        internal static IEnumerable<WebHookWorkItem> GetWorkItems(ICollection<WebHook> webHooks, ICollection<NotificationDictionary> notifications)
        {
            List<WebHookWorkItem> workItems = new List<WebHookWorkItem>();
            foreach (WebHook webHook in webHooks)
            {
                ICollection<NotificationDictionary> webHookNotifications;

                // Pick the notifications that apply for this particular WebHook. If we only got one notification
                // then we know that it applies to all WebHooks. Otherwise each notification may apply only to a subset.
                if (notifications.Count == 1)
                {
                    webHookNotifications = notifications;
                }
                else
                {
                    webHookNotifications = notifications.Where(n => webHook.MatchesAction(n.Action)).ToArray();
                    if (webHookNotifications.Count == 0)
                    {
                        continue;
                    }
                }

                WebHookWorkItem workItem = new WebHookWorkItem(webHook, webHookNotifications);
                workItems.Add(workItem);
            }
            return workItems;
        }

    }
}
