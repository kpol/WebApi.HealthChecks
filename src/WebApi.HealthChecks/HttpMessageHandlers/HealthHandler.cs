using System;
using System.Linq;
using System.Net;
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

            var queryParameters = request.GetQueryNameValuePairs()
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

            if (queryParameters.TryGetValue("check", out var check) && !string.IsNullOrEmpty(check))
            {
                var healthResult = await _healthCheckService.GetHealthAsync(check);

                if (healthResult == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent($"Health check '{check}' is not configured.")
                    };
                }

                return new HttpResponseMessage(_healthCheckService.GetStatusCode(healthResult.Status))
                {
                    Content = new ObjectContent<HealthCheckResultExtended>(healthResult,
                        new JsonMediaTypeFormatter { SerializerSettings = _serializerSettings })
                };
            }

            var result = await _healthCheckService.GetHealthAsync();

            return new HttpResponseMessage(_healthCheckService.GetStatusCode(result.Status))
            {
                Content = new ObjectContent<HealthCheckResults>(result,
                    new JsonMediaTypeFormatter {SerializerSettings = _serializerSettings})
            };
        }
    }
}