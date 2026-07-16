using Workforce.Application.Features.TimeEntries.Dtos;

namespace Workforce.Application.Features.Teams.Dtos;

public sealed record TeamMembersWorkSummaryDto(
    DateOnly FromDate,
    DateOnly ToDate,
    IReadOnlyCollection<TeamMemberWorkSummaryDto> Members);

public sealed record TeamMemberWorkSummaryDto(
    Guid TeamMemberId,
    Guid OrganizationMemberId,
    string TeamMemberStatus,
    IReadOnlyCollection<TeamMemberWorkDayDto> Days,
    TeamMemberWorkTotalsDto Totals);

public sealed record TeamMemberWorkDayDto(
    DateOnly Date,
    IReadOnlyCollection<TeamMemberWorkShiftDto> Shifts);

public sealed record TeamMemberWorkShiftDto(
    Guid ShiftId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    int ShiftDurationMinutes,
    int ShiftBreakDurationMinutes,
    int TimeEntryDurationMinutes,
    int TimeEntryBreakDurationMinutes,
    int WorkedDurationMinutes,
    IReadOnlyCollection<TimeEntryDto> TimeEntries);

public sealed record TeamMemberWorkTotalsDto(
    int TotalShiftCount,
    decimal TotalShiftDurationHours,
    decimal TotalShiftBreakDurationHours,
    int TotalTimeEntryCount,
    decimal TotalTimeEntryDurationHours,
    decimal TotalTimeEntryBreakDurationHours,
    decimal TotalWorkedDurationHours,
    string TotalShiftDurationText,
    string TotalBreakDurationText,
    string TotalWorkedDurationText);
