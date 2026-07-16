using Workforce.Application.Features.OrganizationMembers.Dtos;

namespace Workforce.Application.Features.Teams.Dtos;

public sealed record TeamDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    Guid? ManagerMemberId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    OrganizationMemberDto? ManagerMember);
