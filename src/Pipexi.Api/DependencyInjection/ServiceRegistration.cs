using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workforce.Api.Identity;
using Workforce.Application.Abstractions.Auth;
using Workforce.Application.Abstractions.Identity;
using Workforce.Infrastructure.Services;

namespace Workforce.Api.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddApiSwagger();
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddApiAuthentication(configuration);
        services.AddApiHealthChecks(configuration);

        return services;
    }
}
