using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pipexi.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<Pipexi.Application.Abstractions.Notifications.IPushNotificationService, Pipexi.Infrastructure.Notifications.FirebasePushNotificationService>();
        return services;
    }
}
