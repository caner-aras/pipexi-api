using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pipexi.Api.HealthChecks;

namespace Pipexi.Api.DependencyInjection;

public static class ApiHealthChecksExtensions
{
    public static IServiceCollection AddApiHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgresConnectionString =
            configuration.GetConnectionString("DefaultConnection") ?? configuration["DATABASE_URL"];
        var redisConnectionString = configuration["REDIS_URL"];
        var storageProbeUrl = configuration["HealthChecks:StorageProbeUrl"];

        var healthChecks = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(postgresConnectionString))
        {
            healthChecks.AddNpgSql(
                postgresConnectionString,
                name: "postgres",
                tags: ["db"]);
        }
        else
        {
            healthChecks.AddCheck(
                "postgres",
                () => HealthCheckResult.Unhealthy("PostgreSQL connection string is not configured."),
                tags: ["db"]);
        }

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            healthChecks.AddRedis(
                redisConnectionString,
                name: "redis",
                tags: ["redis"]);
        }
        else
        {
            healthChecks.AddCheck(
                "redis",
                () => HealthCheckResult.Unhealthy("REDIS_URL is not configured."),
                tags: ["redis"]);
        }

        healthChecks.AddCheck<StorageHealthCheck>(
            "storage",
            tags: ["storage"]);

        services.Configure<StorageHealthCheckOptions>(options =>
        {
            options.ProbeUrl = storageProbeUrl;
        });

        return services;
    }
}
