namespace Workforce.Application.Features.RolePermissions.Dtos;

public sealed record RolePermissionDto(
    Guid Id,
    Guid RoleId,
    Guid PermissionId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
