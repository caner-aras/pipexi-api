using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pipexi.Api.Identity;
using Pipexi.Application.Abstractions.Auth;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Identity;
using Pipexi.Infrastructure.Services;

namespace Pipexi.Api.DependencyInjection;

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
        services.AddMemoryCache();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<CurrentUserMembershipState>();
        services.AddScoped<CurrentUserMembershipResolver>();
        services.AddScoped<ICurrentUserMembershipResolver>(sp =>
            new CachingCurrentUserMembershipResolver(
                sp.GetRequiredService<CurrentUserMembershipResolver>(),
                sp.GetRequiredService<IMemoryCache>()));
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddApiAuthentication(configuration);
        services.AddApiHealthChecks(configuration);

        return services;
    }
}
