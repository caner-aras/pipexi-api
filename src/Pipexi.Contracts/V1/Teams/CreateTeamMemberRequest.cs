namespace Workforce.Contracts.V1.Teams;

public sealed record CreateTeamMemberRequest(
    Guid TeamId,
    Guid OrganizationMemberId);
