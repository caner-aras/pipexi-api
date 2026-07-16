using Workforce.Application.Features.OrganizationMembers.Dtos;

namespace Workforce.Application.Features.Teams.Dtos;

public sealed record TeamMemberDto(
    Guid Id,
    Guid TeamId,
    Guid OrganizationMemberId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    TeamDto? Team,
    OrganizationMemberDto? OrganizationMember);
