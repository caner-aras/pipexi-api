namespace Workforce.Application.Features.Reports.Dtos;

public sealed record ReportSummaryDto(
    Guid OrganizationId,
    ReportOverviewDto Overview,
    IReadOnlyCollection<ReportStatusCountDto> TaskStatusDistribution,
    IReadOnlyCollection<ReportStatusCountDto> TaskPriorityDistribution,
    IReadOnlyCollection<ReportStatusCountDto> ShiftStatusDistribution,
    IReadOnlyCollection<ReportStatusCountDto> LeaveStatusDistribution,
    IReadOnlyCollection<ReportDailyActivityDto> DailyActivity,
    IReadOnlyCollection<ReportSignalDto> Signals);

public sealed record ReportOverviewDto(
    int MembersCount,
    int TeamsCount,
    int OpenTasksCount,
    int OverdueTasksCount,
    int TasksCreatedToday,
    int ActiveShiftsNow,
    int CompletedShiftsCount,
    int MembersClockedInToday,
    int PendingLeaveRequests,
    int ApprovedLeaveRequests,
    int FormSubmissionsToday,
    int MissingRequiredShiftForms);

public sealed record ReportStatusCountDto(string Key, int Count);

public sealed record ReportDailyActivityDto(
    DateOnly Date,
    int TasksCreated,
    int TimeEntriesCreated,
    int FormSubmissionsCreated,
    IReadOnlyCollection<ReportDailyShiftAssignmentDto> ShiftAssignments);

public sealed record ReportDailyShiftAssignmentDto(
    Guid ShiftId,
    Guid? OrganizationMemberId,
    string MemberName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    IReadOnlyCollection<ReportShiftTimeEntryDto> TimeEntries);

public sealed record ReportShiftTimeEntryDto(
    Guid TimeEntryId,
    Guid OrganizationMemberId,
    string MemberName,
    TimeOnly ClockInTime,
    TimeOnly? ClockOutTime,
    string Status);

public sealed record ReportSignalDto(string Key, string Label, string Value, string Tone = "default");