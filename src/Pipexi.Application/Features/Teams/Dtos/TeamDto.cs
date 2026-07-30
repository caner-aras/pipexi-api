using Pipexi.Application.Features.OrganizationMembers.Dtos;

namespace Pipexi.Application.Features.Teams.Dtos;

public sealed record TeamDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    Guid? ManagerMemberId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    OrganizationMemberDto? ManagerMember,
    int MemberCount = 0,
    Guid? ManagerTeamMemberId = null);
