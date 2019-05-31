using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WebApi.HealthChecks.Services;

namespace WebApi.HealthChecks.HttpMessageHandlers
{
    internal class HealthUiHandler : HttpMessageHandler
    {
        private const string HealthyImage = "WebApi.HealthChecks.Content.status-healthy-green.svg";
        private const string UnhealthyImage = "WebApi.HealthChecks.Content.status-unhealthy-red.svg";
        private const string DegradedImage = "WebApi.HealthChecks.Content.status-degraded-lightgrey.svg";

        private readonly IHealthCheckService _healthCheckService;
        
        public HealthUiHandler(IHealthCheckService healthCheckService)
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

            HealthStatus status;

            if (routeData.Values.TryGetValue("check", out var check))
            {
                var healthResult = await _healthCheckService.GetHealthAsync((string)check);

                if (healthResult == null)
                {
                    throw new InvalidOperationException($"Health check '{check}' is not configured.");
                }

                status = healthResult.Status;
            }
            else
            {
                var result = await _healthCheckService.GetHealthAsync();
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