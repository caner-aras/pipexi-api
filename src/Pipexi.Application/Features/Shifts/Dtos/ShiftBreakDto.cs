namespace Workforce.Application.Features.Shifts.Dtos;

public sealed record ShiftBreakDto(
    Guid Id,
    Guid ShiftId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsPaid,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
