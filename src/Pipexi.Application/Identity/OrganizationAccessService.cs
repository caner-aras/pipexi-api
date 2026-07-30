using System.Net;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Identity;

public sealed class OrganizationAccessService : IOrganizationAccessService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;

    public OrganizationAccessService(
        ICurrentUserContext currentUserContext,
        IOrganizationMemberRepository organizationMemberRepository)
    {
        _currentUserContext = currentUserContext;
        _organizationMemberRepository = organizationMemberRepository;
    }

    public async Task EnsureMemberAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var violation = await GetMembershipViolationAsync(organizationId, cancellationToken);
        if (violation is not null)
        {
            throw new Common.Exceptions.ForbiddenException(violation.Message);
        }
    }

    public async Task<AppError?> GetMembershipViolationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserContext.UserId == Guid.Empty)
        {
            return new AppError("auth.unauthorized", "Unauthorized.");
        }

        if (organizationId == Guid.Empty)
        {
            return new AppError("auth.organization_required", "Organization is required.");
        }

        var membership = await _organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
            organizationId,
            _currentUserContext.UserId,
            cancellationToken);

        if (membership is null || !IsActiveMembership(membership.Status))
        {
            return new AppError(
                "auth.forbidden",
                "You are not a member of this organization.");
        }

        return null;
    }

    public AppError? GetScopeViolation(Guid resourceOrganizationId, Guid? scopedOrganizationId)
    {
        if (!scopedOrganizationId.HasValue || scopedOrganizationId.Value == Guid.Empty)
        {
            return null;
        }

        if (resourceOrganizationId != scopedOrganizationId.Value)
        {
            return new AppError("resource.not_found", "Resource not found.");
        }

        return null;
    }

    public async Task<Result<T>?> ValidateResourceAccessAsync<T>(
        Guid resourceOrganizationId,
        Guid? scopedOrganizationId,
        CancellationToken cancellationToken = default)
    {
        var scopeViolation = GetScopeViolation(resourceOrganizationId, scopedOrganizationId);
        if (scopeViolation is not null)
        {
            return Result<T>.Failure(scopeViolation, (int)HttpStatusCode.NotFound);
        }

        var membershipViolation = await GetMembershipViolationAsync(
            resourceOrganizationId,
            cancellationToken);

        if (membershipViolation is not null)
        {
            return Result<T>.Failure(membershipViolation, (int)HttpStatusCode.Forbidden);
        }

        return null;
    }

    private static bool IsActiveMembership(string status)
    {
        return string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
    }
}
