using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using WebApi.HealthChecks.Services;

namespace WebApi.HealthChecks.HttpMessageHandlers
{
    internal class HealthUiHandler : HealthHandlerBase
    {
        private const string HealthyImage = "WebApi.HealthChecks.Content.status-healthy-green.svg";
        private const string UnhealthyImage = "WebApi.HealthChecks.Content.status-unhealthy-red.svg";
        private const string DegradedImage = "WebApi.HealthChecks.Content.status-degraded-lightgrey.svg";


        public HealthUiHandler(HttpConfiguration httpConfiguration, HealthChecksBuilder healthChecksBuilder) : base(
            httpConfiguration, healthChecksBuilder)
        {
        }

        protected override async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var healthChecks = GetHealthChecks();
            var service = new HealthCheckService(healthChecks, HealthChecksBuilder.ResultStatusCodes);

            var queryParameters = request.GetQueryNameValuePairs()
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

            HealthStatus status;

            if (queryParameters.TryGetValue("check", out var check) && !string.IsNullOrEmpty(check))
            {
                var healthResult = await service.GetHealthAsync(check);

                if (healthResult == null)
                {
                    return CheckNotFound(check);
                }

                status = healthResult.Status;
            }
            else
            {
                var result = await service.GetHealthAsync();
                status = result.Status;
            }

            return CreateResponse(status);
        }

        private static HttpResponseMessage CreateResponse(HealthStatus status)
        {
            string imageName;

            switch (status)
            {
                case HealthStatus.Healthy:
                    imageName = HealthyImage;
                    break;
                case HealthStatus.Degraded:
                    imageName = DegradedImage;
                    break;
                default:
                    imageName = UnhealthyImage;
                    break;
            }

            var assembly = Assembly.GetExecutingAssembly();

            var httpResponseMessage = new HttpResponseMessage
            {
                Content = new StreamContent(assembly.GetManifestResourceStream(imageName))
            };

            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("image/svg+xml");

            return httpResponseMessage;
        }
    }
}