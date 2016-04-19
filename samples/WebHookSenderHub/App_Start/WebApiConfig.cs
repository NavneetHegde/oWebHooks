using Microsoft.AspNet.WebHooks;
using Microsoft.Practices.Unity;
using System.Web.Http;
using WebHookSenderHub.Unity;
using WebHookSenderHub.WebHookFilters;

namespace WebHookSenderHub
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var container = new UnityContainer();
            container.RegisterType<IWebHookFilterProvider, WebHookFilterProvider>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //enabe cross orgin
            config.EnableCors();

            // Load basic support for sending WebHooks
            config.InitializeCustomWebHooks();

            // Load support for authorized webhook
            config.InitializeAuthenticatedWebHooksSender();

            // Use SQL for persisting subscriptions
            config.InitializeCustomWebHooksSqlStorage();

        }
    }
}
