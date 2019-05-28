using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable once CheckNamespace
namespace WebApi.HealthChecks
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HealthStatus
    {
        Unhealthy = 0,

        Degraded = 1,

        Healthy = 2
    }
}