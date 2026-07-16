namespace Workforce.Contracts.V1.Teams;

public sealed record UpdateTeamRequest(
    string? Name,
    Guid? ManagerMemberId,
    string? Status);
