using System;
using Newtonsoft.Json;

namespace WebApi.HealthChecks
{
    public class HealthCheckResult
    {
        public HealthCheckResult(HealthStatus status, string description = null, Exception exception = null)
        {
            Status = status;
            Description = description;
            Exception = exception;
        }

        public HealthStatus Status { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Exception Exception { get; }
    }
}