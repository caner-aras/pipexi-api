namespace Workforce.Contracts.V1.Tasks;

public sealed record CreateTaskRequest(
    Guid OrganizationId,
    Guid? ShiftId,
    Guid? LocationId,
    string Title,
    string? Description,
    Guid? AssignedToTeamMemberId,
    Guid? AssignedToTeamId,
    DateTimeOffset? DueAt,
    string? Priority);
