using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Common.Behaviors;
using Pipexi.Application.Identity;

namespace Pipexi.Application.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrganizationAccessService, OrganizationAccessService>();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(ServiceRegistration).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        return services;
    }
}
