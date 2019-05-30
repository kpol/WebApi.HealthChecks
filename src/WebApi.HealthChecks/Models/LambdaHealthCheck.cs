using System;
using System.Threading.Tasks;

namespace WebApi.HealthChecks.Models
{
    internal class LambdaHealthCheck : IHealthCheck
    {
        private readonly Func<HealthCheckResult> _check;

        public LambdaHealthCheck(Func<HealthCheckResult> check)
        {
            _check = check;
        }

        public Task<HealthCheckResult> CheckHealthAsync()
        {
            return Task.FromResult(_check());
        }
    }
}