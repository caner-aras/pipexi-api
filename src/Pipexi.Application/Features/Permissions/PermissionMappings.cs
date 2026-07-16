using Workforce.Application.Features.Permissions.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.Permissions;

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
