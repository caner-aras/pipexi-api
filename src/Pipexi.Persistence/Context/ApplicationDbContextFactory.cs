using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Workforce.Persistence.Context;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = TryReadConnectionStringFromAppSettings();
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured for design-time DbContext creation.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string? TryReadConnectionStringFromAppSettings()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var candidateDirectories = new[]
        {
            Directory.GetCurrentDirectory(),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "src/Workforce.Api")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../Workforce.Api"))
        };

        foreach (var directory in candidateDirectories)
        {
            var appSettingsFiles = new[]
            {
                Path.Combine(directory, "appsettings.json"),
                Path.Combine(directory, $"appsettings.{environment}.json")
            };

            foreach (var appSettingsPath in appSettingsFiles)
            {
                if (!File.Exists(appSettingsPath))
                {
                    continue;
                }

                using var stream = File.OpenRead(appSettingsPath);
                using var document = JsonDocument.Parse(stream);
                if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
                {
                    continue;
                }

                if (!connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
                {
                    continue;
                }

                var connectionString = defaultConnection.GetString();
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    return connectionString;
                }
            }
        }

        return null;
    }
}
