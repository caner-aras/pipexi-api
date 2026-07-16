namespace Workforce.Application.Features.TimeEntries.Dtos;

public sealed record TimeEntryBreakDto(
    Guid Id,
    Guid TimeEntryId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsPaid,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
