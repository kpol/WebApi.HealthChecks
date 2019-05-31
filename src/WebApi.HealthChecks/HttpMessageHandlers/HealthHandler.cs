using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApi.HealthChecks.Models;
using WebApi.HealthChecks.Services;

namespace WebApi.HealthChecks.HttpMessageHandlers
{
    internal class HealthHandler : HttpMessageHandler
    {
        private readonly IHealthCheckService _healthCheckService;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public HealthHandler(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
            {
                throw new HttpRequestException("The method accepts only GET requests.");
            }

            var routeData = request.GetRouteData();

            if (routeData.Values.TryGetValue("check", out var check))
            {
                var healthResult = await _healthCheckService.GetHealthAsync((string) check);

                if (healthResult == null)
                {
                    throw new InvalidOperationException($"Health check '{check}' is not configured.");
                }

                return new HttpResponseMessage
                {
                    Content = new ObjectContent<HealthCheckResultExtended>(healthResult,
                        new JsonMediaTypeFormatter { SerializerSettings = _serializerSettings }),
                    StatusCode = _healthCheckService.GetStatusCode(healthResult.Status)
                };
            }

            var result = await _healthCheckService.GetHealthAsync();

            return new HttpResponseMessage
            {
                Content = new ObjectContent<HealthCheckResults>(result,
                    new JsonMediaTypeFormatter {SerializerSettings = _serializerSettings}),
                StatusCode = _healthCheckService.GetStatusCode(result.Status)
            };
        }
    }
}