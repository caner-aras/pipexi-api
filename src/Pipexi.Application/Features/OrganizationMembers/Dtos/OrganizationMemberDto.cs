namespace Workforce.Application.Features.OrganizationMembers.Dtos;

public sealed record OrganizationMemberDto(
    Guid Id,
    Guid OrganizationId,
    Guid UserId,
    Guid RoleId,
    string? JobTitle,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    OrganizationMemberUserDto? User);
