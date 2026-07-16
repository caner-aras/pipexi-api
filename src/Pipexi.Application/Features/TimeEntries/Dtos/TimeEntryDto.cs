namespace Workforce.Application.Features.TimeEntries.Dtos;

public sealed record TimeEntryDto(
    Guid Id,
    Guid OrganizationId,
    Guid ShiftId,
    Guid OrganizationMemberId,
    Guid LocationId,
    DateTimeOffset ClockInAt,
    DateTimeOffset? ClockOutAt,
    string? EmployeeNote,
    string? ManagerNote,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<TimeEntryBreakDto> Breaks);
