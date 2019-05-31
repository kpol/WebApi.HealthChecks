using System.Web.Http;
using WebApi.HealthChecks.HttpMessageHandlers;
using WebApi.HealthChecks.Services;

namespace WebApi.HealthChecks
{
    public static class HttpConfigurationExtensions
    {
        public static HealthChecksBuilder AddHealthChecks(this HttpConfiguration httpConfiguration, string root = "health")
        {
            var healthChecksBuilder = new HealthChecksBuilder();

            var route = root + (root.EndsWith("/") ? string.Empty : "/");

            httpConfiguration.Routes.MapHttpRoute(
                name: "health_check_ui",
                routeTemplate: route + "ui/{check}",
                defaults: new { check = RouteParameter.Optional },
                constraints: null,
                handler: new HealthUiHandler(new HealthCheckService(healthChecksBuilder))
            );

            httpConfiguration.Routes.MapHttpRoute(
                name: "health_check",
                routeTemplate: route + "{check}",
                defaults: new { check = RouteParameter.Optional },
                constraints: null,
                handler: new HealthHandler(new HealthCheckService(healthChecksBuilder))
            );

            return healthChecksBuilder;
        }
    }
}