using Pipexi.Application.Features.RolePermissions.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.RolePermissions;

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
