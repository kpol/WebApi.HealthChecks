using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApi.HealthChecks.Models;

namespace WebApi.HealthChecks.HttpMessageHandlers
{
    internal abstract class HealthHandlerBase : DelegatingHandler
    {
        private readonly HttpConfiguration _httpConfiguration;

        protected HealthHandlerBase(HttpConfiguration httpConfiguration, HealthChecksBuilder healthChecksBuilder)
        {
            _httpConfiguration = httpConfiguration;
            HealthChecksBuilder = healthChecksBuilder;
        }

        protected JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        protected HealthChecksBuilder HealthChecksBuilder { get; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // only accepting GET
            if (request.Method != HttpMethod.Get)
            {
                throw new HttpRequestException("The method accepts only GET requests.");
            }

            return await GetResponseAsync(request, cancellationToken);
        }

        protected abstract Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken);

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
                        var instance = (IHealthCheck)dependencyScope.GetService(registration.Value.Type);

                        result.Add(registration.Key, instance);
                    }
                }

                return result;
            }
        }

        protected HttpResponseMessage CheckNotFound(string check)
            => new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new ObjectContent<ErrorResponse>(
                    new ErrorResponse {Error = $"Health check '{check}' is not configured."},
                    new JsonMediaTypeFormatter {SerializerSettings = SerializerSettings})
            };

        protected void AddWarningHeaderIfNeeded(HttpResponseMessage responseMessage, HealthStatus healthStatus)
        {
            if (HealthChecksBuilder.AddWarningHeader)
            {
                if (healthStatus == HealthStatus.Degraded)
                {
                    responseMessage.Headers.Warning.Add(new WarningHeaderValue(199, "health-check",
                        "\"Status is Degraded\""));
                }
                else if (healthStatus == HealthStatus.Unhealthy)
                {
                    responseMessage.Headers.Warning.Add(new WarningHeaderValue(199, "health-check",
                        "\"Status is Unhealthy\""));
                }
            }
        }
    }
}