using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Workforce.Api.HealthChecks;

public sealed class StorageHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly StorageHealthCheckOptions _options;

    public StorageHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<StorageHealthCheckOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ProbeUrl))
        {
            return HealthCheckResult.Unhealthy("HealthChecks:StorageProbeUrl is not configured.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Head, _options.ProbeUrl);
            using var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy();
            }

            return HealthCheckResult.Unhealthy($"Storage probe returned status {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Storage probe failed.", ex);
        }
    }
}

public sealed class StorageHealthCheckOptions
{
    public string? ProbeUrl { get; set; }
}
