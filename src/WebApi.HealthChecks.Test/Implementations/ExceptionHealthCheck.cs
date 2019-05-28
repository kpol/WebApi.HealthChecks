using System;
using System.Threading.Tasks;

namespace WebApi.HealthChecks.Test.Implementations
{
    public class ExceptionHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync()
        {
            return await Task.FromException<HealthCheckResult>(new InvalidOperationException("The service is down."));
        }
    }
}