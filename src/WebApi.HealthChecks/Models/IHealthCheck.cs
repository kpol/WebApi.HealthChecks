using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace WebApi.HealthChecks
{
    public interface IHealthCheck
    {
        Task<HealthCheckResult> CheckHealthAsync();
    }
}