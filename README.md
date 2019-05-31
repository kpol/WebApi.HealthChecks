# WebApi.HealthChecks

[![Build status](https://ci.appveyor.com/api/projects/status/1g00xtolkwtlt6kh?svg=true)](https://ci.appveyor.com/project/kpol/webapi-healthchecks)
[![Nuget](https://img.shields.io/nuget/v/WebApi.HealthChecks.svg)](https://www.nuget.org/packages/WebApi.HealthChecks)

WebApi.HealthChecks offers a **WebApi** implementation of the health check endpoints for reporting the health of app infrastructure components.

The package is available on [**NuGet**](https://nuget.org/packages/WebApi.HealthChecks)

    PM> Install-Package WebApi.HealthChecks

Health checks are exposed by an app as HTTP endpoints.
Supports two endpoints: 
- `GET /health`
- `GET /health/ui/:check`


By default the health check endpoint is created at `/health`
```
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        config.AddHealthChecks()
            .OverrideResultStatusCodes(unhealthy: HttpStatusCode.InternalServerError)
            .AddCheck("sqlDb", new SqlHealthCheck())
            .AddCheck("cosmosDb", new CosmosDbHealthCheck())
            .AddCheck("lambda", () => new HealthCheckResult(HealthStatus.Healthy, "Lambda check"));
    }
}
```

Every health check must implement `IHealthCheck` interface
```
public interface IHealthCheck
{
    Task<HealthCheckResult> CheckHealthAsync();
}
```
The framework supports three statuses: `Unhealthy` , `Degraded` and `Healthy`.

`GET /health` returns json in the following format:
```
{
  "status": "Degraded",
  "totalResponseTime": 13,
  "entries": {
    "sqlDb": {
      "responseTime": 8,
      "status": "Healthy"
    },
    "cosmosDb": {
      "responseTime": 5,
      "status": "Degraded"
    },
    "lambda": {
      "responseTime": 0,
      "status": "Healthy",
      "description": "Lambda check"
    }
  }
}
```
The `GET /health/ui/:check` endpoint returns a SVG badge which shows individual status of the service component.
For example `GET /health/ui/cosmosDb` will output this image: ![degraded](/src/WebApi.HealthChecks/Content/status-degraded-lightgrey.svg)
