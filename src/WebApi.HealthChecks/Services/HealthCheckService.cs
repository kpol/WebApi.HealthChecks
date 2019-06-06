using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebApi.HealthChecks.Models;

namespace WebApi.HealthChecks.Services
{
    internal class HealthCheckService
    {
        private readonly IDictionary<string, IHealthCheck> _healthChecks;
        private readonly IDictionary<HealthStatus, HttpStatusCode> _resultStatusCodes;

        public HealthCheckService(IDictionary<string, IHealthCheck> healthChecks, IDictionary<HealthStatus, HttpStatusCode> resultStatusCodes)
        {
            _healthChecks = healthChecks;
            _resultStatusCodes = resultStatusCodes;
        }

        public HttpStatusCode GetStatusCode(HealthStatus healthStatus)
        {
            return _resultStatusCodes[healthStatus];
        }

        public async Task<HealthCheckResults> GetHealthAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var healthCheckResults = new HealthCheckResults();

            var tasks = _healthChecks.Select(c => new {name = c.Key, result = c.Value.CheckHealthAsync()});

            var sw = new Stopwatch();

            foreach (var task in tasks)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    sw.Reset();
                    sw.Start();
                    var result = await task.result;
                    sw.Stop();
                    healthCheckResults.Entries.Add(task.name,
                        new HealthCheckResultExtended(result) {ResponseTime = sw.ElapsedMilliseconds});
                }
                catch (OperationCanceledException)
                {
                    throw;
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

        public async Task<HealthCheckResultExtended> GetHealthAsync(string healthCheckName)
        {
            if (!_healthChecks.TryGetValue(healthCheckName, out var healthCheck))
            {
                return null;
            }

            try
            {
                var sw = new Stopwatch();
                sw.Start();

                var result = await healthCheck.CheckHealthAsync();

                sw.Stop();

                return new HealthCheckResultExtended(result) {ResponseTime = sw.ElapsedMilliseconds};
            }
            catch
            {
                return new HealthCheckResultExtended(new HealthCheckResult(HealthStatus.Unhealthy));
            }
        }
    }
}