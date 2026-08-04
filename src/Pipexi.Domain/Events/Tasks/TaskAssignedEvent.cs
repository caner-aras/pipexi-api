namespace Pipexi.Domain.Events.Tasks;

public sealed record TaskAssignedEvent(
    Guid TaskId,
    Guid AssignedToTeamMemberId,
    Guid AssignerUserId,
    string TaskTitle,
    Guid OrganizationId) : IDomainEvent;
