namespace Pipexi.Contracts.V1.TimeEntries;

public sealed record UpdateTimeEntryBreakRequest(
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    bool? IsPaid,
    string? Status);
