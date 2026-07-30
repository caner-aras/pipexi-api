using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;

namespace Pipexi.Application.Identity;

public sealed class CurrentUserMembershipResolver : ICurrentUserMembershipResolver
{
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IRoleRepository _roleRepository;

    public CurrentUserMembershipResolver(
        IOrganizationMemberRepository organizationMemberRepository,
        IRoleRepository roleRepository)
    {
        _organizationMemberRepository = organizationMemberRepository;
        _roleRepository = roleRepository;
    }

    public async Task<CurrentUserMembership?> ResolveAsync(
        Guid userId,
        Guid? requestedOrganizationId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        if (requestedOrganizationId.HasValue && requestedOrganizationId.Value != Guid.Empty)
        {
            var membership = await _organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                requestedOrganizationId.Value,
                userId,
                cancellationToken);

            if (membership is null || !IsUsableMembership(membership.Status))
            {
                return null;
            }

            return await ToMembershipAsync(membership, cancellationToken);
        }

        var memberships = await _organizationMemberRepository.ListByUserIdAsync(userId, cancellationToken);
        var selected = memberships
            .Where(x => IsUsableMembership(x.Status))
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefault();

        if (selected is null)
        {
            return null;
        }

        return await ToMembershipAsync(selected, cancellationToken);
    }

    private async Task<CurrentUserMembership?> ToMembershipAsync(
        Domain.Entities.OrganizationMember membership,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(membership.RoleId, cancellationToken);
        if (role is null)
        {
            return null;
        }

        return new CurrentUserMembership(
            membership.OrganizationId,
            membership.Id,
            role.Id,
            role.Name);
    }

    private static bool IsUsableMembership(string status)
    {
        return string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
    }
}
