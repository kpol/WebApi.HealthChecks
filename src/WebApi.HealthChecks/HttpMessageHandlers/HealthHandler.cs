using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using WebApi.HealthChecks.Services;

namespace WebApi.HealthChecks.HttpMessageHandlers
{
    internal class HealthHandler : HealthHandlerBase
    {
        public HealthHandler(HttpConfiguration httpConfiguration, HealthChecksBuilder healthChecksBuilder) : base(
            httpConfiguration, healthChecksBuilder)
        {
        }

        protected override async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var healthChecks = GetHealthChecks();
            var service = new HealthCheckService(healthChecks, HealthChecksBuilder.ResultStatusCodes);

            var queryParameters = request.GetQueryNameValuePairs()
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

            if (queryParameters.TryGetValue("check", out var check) && !string.IsNullOrEmpty(check))
            {
                var healthResult = await service.GetHealthAsync(check);

                if (healthResult == null)
                {
                    return CheckNotFound(check);
                }

                return GetResponse(healthResult, healthResult.Status, service);
            }

            var result = await service.GetHealthAsync(cancellationToken);

            return GetResponse(result, result.Status, service);
        }

        private HttpResponseMessage GetResponse<T>(T objectContent, HealthStatus healthStatus, HealthCheckService healthCheckService)
        {
            var response = new HttpResponseMessage(healthCheckService.GetStatusCode(healthStatus))
            {
                Content = new ObjectContent<T>(objectContent,
                    new JsonMediaTypeFormatter {SerializerSettings = SerializerSettings})
            };
            
            if (healthStatus == HealthStatus.Degraded)
            {
                response.Headers.Warning.Add(new WarningHeaderValue(199, "health-check", "\"Status is Degraded\""));
            }
            else if(healthStatus == HealthStatus.Unhealthy)
            {
                response.Headers.Warning.Add(new WarningHeaderValue(199, "health-check", "\"Status is Unhealthy\""));
            }

            return response;
        }
    }
}