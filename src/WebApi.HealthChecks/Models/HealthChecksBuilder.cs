using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApi.HealthChecks.Models;

// ReSharper disable once CheckNamespace
namespace WebApi.HealthChecks
{
    public class HealthChecksBuilder
    {
        private readonly Dictionary<string, IHealthCheck> _healthChecks =
            new Dictionary<string, IHealthCheck>(StringComparer.OrdinalIgnoreCase);

        internal HealthChecksBuilder()
        {

        }

        public HealthChecksBuilder AddCheck(string name, IHealthCheck healthCheck)
        {
            _healthChecks.Add(name, healthCheck);

            return this;
        }

        internal async Task<HealthCheckResults> GetHealthAsync()
        {
            var healthCheckResults = new HealthCheckResults();

            var tasks = _healthChecks.Select(c => new {name = c.Key, result = c.Value.CheckHealthAsync()});

            var sw = new Stopwatch();

            foreach (var task in tasks)
            {
                try
                {
                    sw.Reset();
                    sw.Start();
                    var result = await task.result;
                    sw.Stop();
                    healthCheckResults.Entries.Add(task.name,
                        new HealthCheckResultExtended(result) {ResponseTime = sw.ElapsedMilliseconds});
                }
                catch
                {
                    healthCheckResults.Entries.Add(task.name,
                        new HealthCheckResultExtended(new HealthCheckResult(HealthStatus.Unhealthy)));
                }
            }

            var status = HealthStatus.Healthy;

            foreach (var healthCheckResultExtended in healthCheckResults.Entries.Values)
            {
                if (healthCheckResultExtended.Status == HealthStatus.Unhealthy)
                {
                    status = HealthStatus.Unhealthy;
                    break;
                }

                if (healthCheckResultExtended.Status == HealthStatus.Degraded)
                {
                    status = HealthStatus.Degraded;
                }
            }

            healthCheckResults.Status = status;
            healthCheckResults.TotalResponseTime = healthCheckResults.Entries.Values.Sum(c => c.ResponseTime);

            return healthCheckResults;
        }

        internal async Task<HealthCheckResult> GetHealthAsync(string healthCheckName)
        {
            if (!_healthChecks.TryGetValue(healthCheckName, out var healthCheck))
            {
                return null;
            }

            try
            {
                var result = await healthCheck.CheckHealthAsync();

                return result;
            }
            catch
            {
                return new HealthCheckResult(HealthStatus.Unhealthy);
            }
        }
    }
}