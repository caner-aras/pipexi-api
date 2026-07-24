namespace Pipexi.Application.Features.Permissions.Dtos;

public sealed record PermissionDto(
    Guid Id,
    string Key,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
