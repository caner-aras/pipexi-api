using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.TimeEntries;

internal static class TimeEntryMappings
{
    public static TimeEntryDto ToDto(this TimeEntry timeEntry, IReadOnlyCollection<TimeEntryBreakDto>? breaks = null)
    {
        return new TimeEntryDto(
            timeEntry.Id,
            timeEntry.OrganizationId,
            timeEntry.ShiftId,
            timeEntry.OrganizationMemberId,
            timeEntry.LocationId,
            timeEntry.ClockInAt,
            timeEntry.ClockOutAt,
            timeEntry.EmployeeNote,
            timeEntry.ManagerNote,
            timeEntry.Status,
            timeEntry.CreatedAt,
            timeEntry.UpdatedAt,
            breaks ?? Array.Empty<TimeEntryBreakDto>());
    }

    public static TimeEntryBreakDto ToDto(this TimeEntryBreak timeEntryBreak)
    {
        return new TimeEntryBreakDto(
            timeEntryBreak.Id,
            timeEntryBreak.TimeEntryId,
            timeEntryBreak.StartAt,
            timeEntryBreak.EndAt,
            timeEntryBreak.IsPaid,
            timeEntryBreak.Status,
            timeEntryBreak.CreatedAt,
            timeEntryBreak.UpdatedAt);
    }
}
