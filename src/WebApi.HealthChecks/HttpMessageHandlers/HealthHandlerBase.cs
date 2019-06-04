using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;

namespace WebApi.HealthChecks.HttpMessageHandlers
{
    internal abstract class HealthHandlerBase : HttpMessageHandler
    {
        private readonly HttpConfiguration _httpConfiguration;

        protected HealthHandlerBase(HttpConfiguration httpConfiguration, HealthChecksBuilder healthChecksBuilder)
        {
            _httpConfiguration = httpConfiguration;
            HealthChecksBuilder = healthChecksBuilder;
        }

        protected HealthChecksBuilder HealthChecksBuilder { get; }

        protected IDictionary<string, IHealthCheck> GetHealthChecks()
        {
            using (var dependencyScope = _httpConfiguration.DependencyResolver.BeginScope())
            {
                var result = new Dictionary<string, IHealthCheck>();

                foreach (var registration in HealthChecksBuilder.HealthChecks)
                {
                    if (registration.Value.IsSingleton)
                    {
                        result.Add(registration.Key, registration.Value.Instance);
                    }
                    else
                    {
                        var instance = (IHealthCheck) dependencyScope.GetService(registration.Value.Type);

                        result.Add(registration.Key, instance);
                    }
                }

                return result;
            }
        }
    }
}