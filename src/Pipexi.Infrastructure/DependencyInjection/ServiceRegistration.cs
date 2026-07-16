using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Workforce.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration;
        return services;
    }
}
