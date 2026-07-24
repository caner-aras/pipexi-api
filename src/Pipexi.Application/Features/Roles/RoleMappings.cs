using Pipexi.Application.Features.Roles.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Roles;

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
