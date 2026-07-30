using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Common.Authorization;
using Pipexi.Application.Common.Exceptions;

namespace Pipexi.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IOrganizationAccessService _organizationAccessService;
    private readonly ICurrentUserContext _currentUserContext;

    public AuthorizationBehavior(
        IOrganizationAccessService organizationAccessService,
        ICurrentUserContext currentUserContext)
    {
        _organizationAccessService = organizationAccessService;
        _currentUserContext = currentUserContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IIgnoreOrganizationAuthorization)
        {
            return await next();
        }

        if (!OrganizationIdResolver.RequiresOrganizationContext(request))
        {
            return await next();
        }

        var organizationId = OrganizationIdResolver.Resolve(request, _currentUserContext);
        if (!organizationId.HasValue || organizationId.Value == Guid.Empty)
        {
            throw new ForbiddenException("Organization context is required.");
        }

        await _organizationAccessService.EnsureMemberAsync(organizationId.Value, cancellationToken);

        return await next();
    }
}
