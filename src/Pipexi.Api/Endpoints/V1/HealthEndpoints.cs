using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Workforce.Api.Endpoints.V1;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteResponseAsync
        });

        app.MapHealthChecks("/health/db", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("db"),
            ResponseWriter = WriteResponseAsync
        });

        app.MapHealthChecks("/health/redis", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("redis"),
            ResponseWriter = WriteResponseAsync
        });

        app.MapHealthChecks("/health/storage", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("storage"),
            ResponseWriter = WriteResponseAsync
        });

        return app;
    }

    private static Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            })
        });

        return context.Response.WriteAsync(payload);
    }
}
