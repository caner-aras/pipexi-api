namespace Pipexi.Contracts.V1.TimeEntries;

public sealed record CreateTimeEntryBreakRequest(
    Guid TimeEntryId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsPaid);
