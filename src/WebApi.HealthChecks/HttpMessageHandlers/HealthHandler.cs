using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using WebApi.HealthChecks.Models;
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

                return new HttpResponseMessage(service.GetStatusCode(healthResult.Status))
                {
                    Content = new ObjectContent<HealthCheckResultExtended>(healthResult,
                        new JsonMediaTypeFormatter {SerializerSettings = SerializerSettings })
                };
            }

            var result = await service.GetHealthAsync(cancellationToken);

            return new HttpResponseMessage(service.GetStatusCode(result.Status))
            {
                Content = new ObjectContent<HealthCheckResults>(result,
                    new JsonMediaTypeFormatter {SerializerSettings = SerializerSettings })
            };
        }
    }
}