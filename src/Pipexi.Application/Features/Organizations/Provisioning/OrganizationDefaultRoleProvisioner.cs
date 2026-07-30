using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Organizations.Provisioning;

public static class OrganizationDefaultRoleProvisioner
{
    public static async Task<IReadOnlyCollection<Role>> EnsureDefaultRolesAsync(
        IRoleRepository roleRepository,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var existing = await roleRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
        var existingNames = existing
            .Select(role => role.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = OrganizationProvisioningDefaults.StaticRoles
            .Select(role => role.ToRoleName())
            .Where(name => !existingNames.Contains(name))
            .Select(name => Role.Create(organizationId, name))
            .ToList();

        if (missing.Count > 0)
        {
            await roleRepository.AddRangeAsync(missing, cancellationToken);
            return existing.Concat(missing).ToList();
        }

        return existing;
    }
}
