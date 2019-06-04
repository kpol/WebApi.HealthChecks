using System.Net;
using System.Web.Http;
using Unity;
using Unity.AspNet.WebApi;
using WebApi.HealthChecks.Test.Implementations;
using WebApi.HealthChecks.Test.Services;

namespace WebApi.HealthChecks.Test
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var container = new UnityContainer();

            config.DependencyResolver = new UnityDependencyResolver(container);

            config
                .AddHealthChecks()
                .OverrideResultStatusCodes(unhealthy: HttpStatusCode.InternalServerError)
                .AddCheck("check1", new HealthyCheck())
                .AddCheck("check2", new UnhealthyCheck())
                .AddCheck("check3", new ExceptionHealthCheck())
                .AddCheck<DegradedHealthCheck>("check4")
                .AddCheck("ui", () => new HealthCheckResult(HealthStatus.Healthy, "Lambda check"));

            container.RegisterType<ICosmosClient, CosmosClient>();
        }
    }
}