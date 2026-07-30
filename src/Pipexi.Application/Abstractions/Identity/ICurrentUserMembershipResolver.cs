namespace Pipexi.Application.Abstractions.Identity;

public interface ICurrentUserMembershipResolver
{
    Task<CurrentUserMembership?> ResolveAsync(
        Guid userId,
        Guid? requestedOrganizationId,
        CancellationToken cancellationToken = default);
}
