using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Application.Features.Teams.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.Teams;

internal static class TeamMappings
{
    public static TeamDto ToDto(this Team team, OrganizationMemberDto? managerMember = null)
    {
        return new TeamDto(
            team.Id,
            team.OrganizationId,
            team.Name,
            team.ManagerMemberId,
            team.Status,
            team.CreatedAt,
            team.UpdatedAt,
            managerMember);
    }

    public static TeamMemberDto ToDto(
        this TeamMember teamMember,
        TeamDto? team = null,
        OrganizationMemberDto? organizationMember = null)
    {
        return new TeamMemberDto(
            teamMember.Id,
            teamMember.TeamId,
            teamMember.OrganizationMemberId,
            teamMember.Status,
            teamMember.CreatedAt,
            teamMember.UpdatedAt,
            team,
            organizationMember);
    }

    public static TeamMemberDayOffDto ToDto(this TeamMemberDayOff dayOff)
    {
        return new TeamMemberDayOffDto(
            dayOff.Id,
            dayOff.TeamMemberId,
            dayOff.StartAt,
            dayOff.EndAt,
            dayOff.Reason,
            dayOff.Status,
            dayOff.CreatedAt,
            dayOff.UpdatedAt);
    }
}
