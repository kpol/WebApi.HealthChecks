﻿using System.Net;
using System.Web.Http;
using WebApi.HealthChecks.Test.Implementations;

namespace WebApi.HealthChecks.Test
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .AddHealthChecks()
                .OverrideResultStatusCodes(unhealthy: HttpStatusCode.InternalServerError)
                .AddCheck("check1", new HealthyCheck())
                .AddCheck("check2", new UnhealthyCheck())
                .AddCheck("check3", new ExceptionHealthCheck())
                .AddCheck("check5", new DegradedHealthCheck())
                .AddCheck("check6", () => new HealthCheckResult(HealthStatus.Healthy, "Lambda check"));
        }
    }
}