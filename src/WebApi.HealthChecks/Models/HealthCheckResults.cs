using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebApi.HealthChecks.Models
{
    internal class HealthCheckResults
    {
        public HealthStatus Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalResponseTime { get; set; }

        public IDictionary<string, HealthCheckResultExtended> Entries { get; } =
            new Dictionary<string, HealthCheckResultExtended>();
    }
}