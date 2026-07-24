namespace Pipexi.Contracts.V1.Teams;

public sealed record CreateTeamRequest(
    Guid OrganizationId,
    string Name,
    Guid? ManagerMemberId);
