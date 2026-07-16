using Workforce.Application.Features.Roles.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.Roles;

internal static class RoleMappings
{
    public static RoleDto ToDto(this Role role)
    {
        return new RoleDto(
            role.Id,
            role.OrganizationId,
            role.Name,
            role.Status,
            role.CreatedAt,
            role.UpdatedAt);
    }
}
