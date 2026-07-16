namespace Workforce.Contracts.V1.Shifts;

public sealed record CreateShiftBreakRequest(
    Guid ShiftId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsPaid);
