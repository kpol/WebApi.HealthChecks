using System;
using System.Collections.Generic;
using System.Net;
using WebApi.HealthChecks.Models;

namespace WebApi.HealthChecks
{
    public class HealthChecksBuilder
    {
        internal HealthChecksBuilder()
        {

        }

        internal IDictionary<HealthStatus, HttpStatusCode> ResultStatusCodes { get; } =
            new Dictionary<HealthStatus, HttpStatusCode>(3)
            {
                {HealthStatus.Healthy, HttpStatusCode.OK},
                {HealthStatus.Degraded, HttpStatusCode.OK},
                {HealthStatus.Unhealthy, HttpStatusCode.ServiceUnavailable}
            };

        internal IDictionary<string, IHealthCheck> HealthChecks { get; } =
            new Dictionary<string, IHealthCheck>(StringComparer.OrdinalIgnoreCase);

        public HealthChecksBuilder AddCheck(string name, IHealthCheck healthCheck)
        {
            HealthChecks.Add(name, healthCheck);

            return this;
        }

        public HealthChecksBuilder AddCheck(string name, Func<HealthCheckResult> check)
        {
            HealthChecks.Add(name, new LambdaHealthCheck(check));

            return this;
        }

        public HealthChecksBuilder OverrideResultStatusCodes(HttpStatusCode healthy = HttpStatusCode.OK,
            HttpStatusCode degraded = HttpStatusCode.OK, HttpStatusCode unhealthy = HttpStatusCode.ServiceUnavailable)
        {
            ResultStatusCodes[HealthStatus.Healthy] = healthy;
            ResultStatusCodes[HealthStatus.Degraded] = degraded;
            ResultStatusCodes[HealthStatus.Unhealthy] = unhealthy;

            return this;
        }
    }
}