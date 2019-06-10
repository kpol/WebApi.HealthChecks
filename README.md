# WebApi.HealthChecks

[![Build status](https://ci.appveyor.com/api/projects/status/1g00xtolkwtlt6kh?svg=true)](https://ci.appveyor.com/project/kpol/webapi-healthchecks)
[![Nuget](https://img.shields.io/nuget/v/WebApi.HealthChecks.svg)](https://www.nuget.org/packages/WebApi.HealthChecks)

WebApi.HealthChecks offers a **WebApi** implementation of the health check endpoints for reporting the health of app infrastructure components.

The package is available on [**NuGet**](https://nuget.org/packages/WebApi.HealthChecks)

    PM> Install-Package WebApi.HealthChecks

Health checks are exposed by an app as HTTP endpoints.
Supports two endpoints: 
- `GET /health?check=:check` *where* `check` *is optional*
- `GET /health/ui?check=:check` *where* `check` *is optional*


By default the health check endpoint is created at `/health`
```
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        config.AddHealthChecks()
            .Configure(addWarningHeader: true) // optional configuration
            .OverrideResultStatusCodes(unhealthy: HttpStatusCode.InternalServerError) // optional configuration
            .AddCheck("sqlDb", new SqlHealthCheck()) // Singleton instance
            .AddCheck<ICosmosDbCheck>("cosmosDb") // needs to be registered in DependencyResolver
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
The `GET /health/ui?check=:check` endpoint returns a SVG badge which shows individual status of the service component.
For example `GET /health/ui?check=cosmosDb` will output this image: ![degraded](/src/WebApi.HealthChecks/Content/status-degraded-lightgrey.svg)
