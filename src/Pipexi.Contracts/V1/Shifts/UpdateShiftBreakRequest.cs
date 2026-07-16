namespace Workforce.Contracts.V1.Shifts;

public sealed record UpdateShiftBreakRequest(
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    bool? IsPaid,
    string? Status);
