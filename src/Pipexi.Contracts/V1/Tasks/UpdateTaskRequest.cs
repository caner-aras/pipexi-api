namespace Pipexi.Contracts.V1.Tasks;

public sealed record UpdateTaskRequest(
    Guid? ShiftId,
    Guid? LocationId,
    string? Title,
    string? Description,
    Guid? AssignedToTeamMemberId,
    Guid? AssignedToTeamId,
    DateTimeOffset? DueAt,
    string? Priority,
    string? Status);
