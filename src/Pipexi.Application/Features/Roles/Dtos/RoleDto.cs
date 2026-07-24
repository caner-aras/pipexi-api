namespace Pipexi.Application.Features.Roles.Dtos;

public sealed record RoleDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
