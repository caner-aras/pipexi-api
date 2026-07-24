using Pipexi.Application.Features.Permissions.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Permissions;

internal static class PermissionMappings
{
    public static PermissionDto ToDto(this Permission permission)
    {
        return new PermissionDto(
            permission.Id,
            permission.Key,
            permission.Status,
            permission.CreatedAt,
            permission.UpdatedAt);
    }
}
