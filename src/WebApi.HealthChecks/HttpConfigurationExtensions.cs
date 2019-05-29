using System.Web.Http;
using WebApi.HealthChecks.HttpMessageHandlers;

namespace WebApi.HealthChecks
{
    public static class HttpConfigurationExtensions
    {
        public static HealthChecksBuilder AddHealthChecks(this HttpConfiguration httpConfiguration, string routeTemplate = "health")
        {
            var healthChecksBuilder = new HealthChecksBuilder();

            httpConfiguration.Routes.MapHttpRoute(
                name: "health_check",
                routeTemplate: routeTemplate,
                defaults: null,
                constraints: null,
                handler: new HealthHandler(healthChecksBuilder)
            );

            var ui = routeTemplate + (routeTemplate.EndsWith("/") ? string.Empty : "/") + "ui";

            httpConfiguration.Routes.MapHttpRoute(
                name: "health_check_ui",
                routeTemplate: ui,
                defaults: null,
                constraints: null,
                handler: new HealthUiHandler(healthChecksBuilder)
            );

            return healthChecksBuilder;
        }
    }
}