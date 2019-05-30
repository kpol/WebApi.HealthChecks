using System.Threading.Tasks;

namespace WebApi.HealthChecks
{
    public interface IHealthCheck
    {
        Task<HealthCheckResult> CheckHealthAsync();
    }
}