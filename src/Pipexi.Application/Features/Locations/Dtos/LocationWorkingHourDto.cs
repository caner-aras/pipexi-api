namespace Workforce.Application.Features.Locations.Dtos;

public sealed record LocationWorkingHourDto(
    Guid Id,
    Guid LocationId,
    int DayOfWeek,
    bool IsClosed,
    TimeOnly? OpensAt,
    TimeOnly? ClosesAt,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
