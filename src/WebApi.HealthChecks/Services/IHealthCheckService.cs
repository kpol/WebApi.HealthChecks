using System.Net;
using System.Threading.Tasks;
using WebApi.HealthChecks.Models;

namespace WebApi.HealthChecks.Services
{
    internal interface IHealthCheckService
    {
        HttpStatusCode GetStatusCode(HealthStatus healthStatus);

        Task<HealthCheckResults> GetHealthAsync();

        Task<HealthCheckResult> GetHealthAsync(string healthCheckName);
    }
}