using Workforce.Application.Features.RolePermissions.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.RolePermissions;

internal static class RolePermissionMappings
{
    public static RolePermissionDto ToDto(this RolePermission rolePermission)
    {
        return new RolePermissionDto(
            rolePermission.Id,
            rolePermission.RoleId,
            rolePermission.PermissionId,
            rolePermission.Status,
            rolePermission.CreatedAt,
            rolePermission.UpdatedAt);
    }
}
