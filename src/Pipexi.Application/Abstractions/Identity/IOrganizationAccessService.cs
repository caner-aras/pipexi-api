using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Abstractions.Identity;

public interface IOrganizationAccessService
{
    Task EnsureMemberAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<AppError?> GetMembershipViolationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    AppError? GetScopeViolation(Guid resourceOrganizationId, Guid? scopedOrganizationId);

    Task<Result<T>?> ValidateResourceAccessAsync<T>(
        Guid resourceOrganizationId,
        Guid? scopedOrganizationId,
        CancellationToken cancellationToken = default);
}
