using System.Web.Http;
using WebApi.HealthChecks.HttpMessageHandlers;
using WebApi.HealthChecks.Services;

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
                handler: new HealthHandler(new HealthCheckService(healthChecksBuilder))
            );

            var ui = routeTemplate + (routeTemplate.EndsWith("/") ? string.Empty : "/") + "ui/{check}";

            httpConfiguration.Routes.MapHttpRoute(
                name: "health_check_ui",
                routeTemplate: ui,
                defaults: new {check = RouteParameter.Optional},
                constraints: null,
                handler: new HealthUiHandler(new HealthCheckService(healthChecksBuilder))
            );

            return healthChecksBuilder;
        }
    }
}