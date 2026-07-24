namespace Pipexi.Contracts.V1.TimeEntries;

public sealed record CreateTimeEntryRequest(
    Guid OrganizationId,
    Guid ShiftId,
    Guid OrganizationMemberId,
    Guid LocationId,
    DateTimeOffset ClockInAt,
    DateTimeOffset? ClockOutAt,
    string? EmployeeNote,
    string? ManagerNote,
    IReadOnlyCollection<CreateTimeEntryPlannedBreakRequest>? Breaks);

public sealed record CreateTimeEntryPlannedBreakRequest(
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsPaid);
