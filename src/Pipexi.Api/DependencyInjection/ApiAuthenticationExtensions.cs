using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Workforce.Api.DependencyInjection;

public static class ApiAuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var supabaseAuthority = configuration["Supabase:Auth:Authority"] ?? string.Empty;
        var supabaseJwksAddress = configuration["Supabase:Auth:JwksAddress"] ?? string.Empty;
        var supabaseAudience = configuration["Supabase:Auth:Audience"] ?? "authenticated";

        var normalizedAuthority = supabaseAuthority.TrimEnd('/');
        var openIdConfigurationAddress =
            string.IsNullOrWhiteSpace(normalizedAuthority)
                ? string.Empty
                : $"{normalizedAuthority}/.well-known/openid-configuration";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = normalizedAuthority;

                options.MetadataAddress =
                    !string.IsNullOrWhiteSpace(openIdConfigurationAddress)
                        ? openIdConfigurationAddress
                        : supabaseJwksAddress;
                options.IncludeErrorDetails = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = string.IsNullOrWhiteSpace(normalizedAuthority)
                        ? null
                        : [normalizedAuthority, $"{normalizedAuthority}/"],
                    ValidateAudience = true,
                    ValidAudience = supabaseAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");

                        logger.LogError(context.Exception, "JWT validation failed.");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
