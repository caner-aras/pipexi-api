using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Reports.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Reports.Queries.GetReportSummary;

public sealed record GetReportSummaryQuery(Guid OrganizationId, int TrendDays = 7, int FutureDays = 7)
    : IQuery<Result<ReportSummaryDto>>;

public sealed class Handler : IRequestHandler<GetReportSummaryQuery, Result<ReportSummaryDto>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IWorkTaskRepository _workTaskRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IFormSubmissionRepository _formSubmissionRepository;
    private readonly IShiftRequiredFormTemplateRepository _shiftRequiredFormTemplateRepository;
    private readonly IUserRepository _userRepository;

    public Handler(
        IOrganizationRepository organizationRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        ITeamRepository teamRepository,
        IWorkTaskRepository workTaskRepository,
        IShiftRepository shiftRepository,
        ITimeEntryRepository timeEntryRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IFormSubmissionRepository formSubmissionRepository,
        IShiftRequiredFormTemplateRepository shiftRequiredFormTemplateRepository,
        IUserRepository userRepository)
    {
        _organizationRepository = organizationRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _teamRepository = teamRepository;
        _workTaskRepository = workTaskRepository;
        _shiftRepository = shiftRepository;
        _timeEntryRepository = timeEntryRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _formSubmissionRepository = formSubmissionRepository;
        _shiftRequiredFormTemplateRepository = shiftRequiredFormTemplateRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<ReportSummaryDto>> Handle(GetReportSummaryQuery request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
        if (organization is null)
        {
            return Result<ReportSummaryDto>.Failure(
                new AppError("reports.organization_not_found", "Organization not found."),
                (int)HttpStatusCode.NotFound);
        }

        var trendDays = Math.Clamp(request.TrendDays, 7, 90);
        var futureDays = Math.Clamp(request.FutureDays, 0, 30);
        var now = DateTimeOffset.UtcNow;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);
        var trendStart = todayStart.AddDays(-(trendDays - 1));
        var totalActivityDays = trendDays + futureDays;

        // Use sequential repository calls because repositories share one scoped DbContext.
        var members = await _organizationMemberRepository
            .ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var memberUserIds = members
            .Select(x => x.UserId)
            .Distinct()
            .ToList();

        var users = memberUserIds.Count == 0
            ? Array.Empty<Workforce.Domain.Entities.User>()
            : await _userRepository.ListByIdsAsync(memberUserIds, cancellationToken);
        var teams = await _teamRepository
            .ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var tasks = await _workTaskRepository
            .ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var shifts = await _shiftRepository
            .ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var timeEntries = await _timeEntryRepository
            .ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var leaveRequests = await _leaveRequestRepository
            .ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var formSubmissions = await _formSubmissionRepository
            .ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);

        var openTasksCount = tasks.Count(x => x.Status is "open" or "in_progress" or "blocked");
        var overdueTasksCount = tasks.Count(x =>
            x.DueAt.HasValue &&
            x.DueAt.Value < now &&
            x.Status is not "done" and not "completed" and not "canceled" and not "cancelled");

        var activeShiftsNow = shifts.Count(x => x.StartAt <= now && x.EndAt >= now && x.Status == "active");
        var completedShiftsCount = shifts.Count(x => x.Status is "completed" or "passed");

        var pendingLeaveRequests = leaveRequests.Count(x => x.Status == "pending");
        var approvedLeaveRequests = leaveRequests.Count(x => x.Status == "approved");

        var membersClockedInToday = timeEntries
            .Where(x => x.ClockInAt >= todayStart && x.ClockInAt < todayEnd)
            .Select(x => x.OrganizationMemberId)
            .Distinct()
            .Count();

        var missingRequiredShiftForms = await CountMissingRequiredShiftFormsAsync(shifts, cancellationToken);

        var overview = new ReportOverviewDto(
            MembersCount: members.Count,
            TeamsCount: teams.Count,
            OpenTasksCount: openTasksCount,
            OverdueTasksCount: overdueTasksCount,
            TasksCreatedToday: tasks.Count(x => x.CreatedAt >= todayStart && x.CreatedAt < todayEnd),
            ActiveShiftsNow: activeShiftsNow,
            CompletedShiftsCount: completedShiftsCount,
            MembersClockedInToday: membersClockedInToday,
            PendingLeaveRequests: pendingLeaveRequests,
            ApprovedLeaveRequests: approvedLeaveRequests,
            FormSubmissionsToday: formSubmissions.Count(x => x.CreatedAt >= todayStart && x.CreatedAt < todayEnd),
            MissingRequiredShiftForms: missingRequiredShiftForms);

        var userNameById = users.ToDictionary(
            x => x.Id,
            x => string.IsNullOrWhiteSpace($"{x.FirstName} {x.LastName}")
                ? x.Email
                : $"{x.FirstName} {x.LastName}".Trim());

        var memberNameByOrganizationMemberId = members.ToDictionary(
            x => x.Id,
            x => userNameById.GetValueOrDefault(x.UserId, "Unknown"));

        var dto = new ReportSummaryDto(
            request.OrganizationId,
            overview,
            BuildStatusDistribution(tasks.Select(x => x.Status)),
            BuildStatusDistribution(tasks.Select(x => x.Priority)),
            BuildStatusDistribution(shifts.Select(x => x.Status)),
            BuildStatusDistribution(leaveRequests.Select(x => x.Status)),
            BuildDailyActivity(tasks, timeEntries, formSubmissions, shifts, memberNameByOrganizationMemberId, trendStart, totalActivityDays),
            BuildSignals(overview));

        return Result<ReportSummaryDto>.Success(dto);
    }

    private async Task<int> CountMissingRequiredShiftFormsAsync(
        IReadOnlyCollection<Workforce.Domain.Entities.Shift> shifts,
        CancellationToken cancellationToken)
    {
        var missingCount = 0;
        foreach (var shift in shifts)
        {
            var requiredTemplateIds = await _shiftRequiredFormTemplateRepository
                .ListRequiredTemplateIdsByShiftIdAsync(shift.Id, cancellationToken);

            if (requiredTemplateIds.Count == 0)
            {
                continue;
            }

            var submittedTemplateIds = await _formSubmissionRepository
                .ListSubmittedTemplateIdsByShiftAsync(shift.Id, cancellationToken);

            if (requiredTemplateIds.Except(submittedTemplateIds).Any())
            {
                missingCount++;
            }
        }

        return missingCount;
    }

    private static IReadOnlyCollection<ReportStatusCountDto> BuildStatusDistribution(IEnumerable<string> values)
    {
        return values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .GroupBy(x => x)
            .Select(g => new ReportStatusCountDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Key)
            .ToList();
    }

    private static IReadOnlyCollection<ReportDailyActivityDto> BuildDailyActivity(
        IReadOnlyCollection<Workforce.Domain.Entities.WorkTask> tasks,
        IReadOnlyCollection<Workforce.Domain.Entities.TimeEntry> timeEntries,
        IReadOnlyCollection<Workforce.Domain.Entities.FormSubmission> formSubmissions,
        IReadOnlyCollection<Workforce.Domain.Entities.Shift> shifts,
        IReadOnlyDictionary<Guid, string> memberNameByOrganizationMemberId,
        DateTimeOffset trendStart,
        int trendDays)
    {
        var taskMap = tasks
            .Where(x => x.CreatedAt >= trendStart)
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAt.UtcDateTime.Date))
            .ToDictionary(g => g.Key, g => g.Count());

        var timeEntryMap = timeEntries
            .Where(x => x.CreatedAt >= trendStart)
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAt.UtcDateTime.Date))
            .ToDictionary(g => g.Key, g => g.Count());

        var submissionMap = formSubmissions
            .Where(x => x.CreatedAt >= trendStart)
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAt.UtcDateTime.Date))
            .ToDictionary(g => g.Key, g => g.Count());

        var timeEntriesByShiftId = timeEntries
            .GroupBy(x => x.ShiftId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var points = new List<ReportDailyActivityDto>(trendDays);
        for (var day = 0; day < trendDays; day++)
        {
            var date = DateOnly.FromDateTime(trendStart.UtcDateTime.Date.AddDays(day));
            var dayStart = trendStart.AddDays(day);
            var dayEnd = dayStart.AddDays(1);

            var shiftAssignments = shifts
                .Where(x => x.StartAt < dayEnd && x.EndAt > dayStart)
                .Select(x =>
                {
                    var clippedStart = x.StartAt > dayStart ? x.StartAt : dayStart;
                    var clippedEnd = x.EndAt < dayEnd ? x.EndAt : dayEnd;
                    return new
                    {
                        Shift = x,
                        Start = clippedStart,
                        End = clippedEnd
                    };
                })
                .Where(x => x.Start < x.End)
                .OrderBy(x => x.Start)
                .ThenBy(x => x.End)
                .Select(x => new ReportDailyShiftAssignmentDto(
                    x.Shift.Id,
                    x.Shift.OrganizationMemberId,
                    x.Shift.OrganizationMemberId.HasValue
                        ? memberNameByOrganizationMemberId.GetValueOrDefault(x.Shift.OrganizationMemberId.Value, "Unknown")
                        : "Unassigned",
                    TimeOnly.FromDateTime(x.Start.UtcDateTime),
                    TimeOnly.FromDateTime(x.End.UtcDateTime),
                    (timeEntriesByShiftId.GetValueOrDefault(x.Shift.Id) ?? [])
                        .Select(te =>
                        {
                            var entryStart = te.ClockInAt;
                            var entryEnd = te.ClockOutAt ?? dayEnd;
                            var clippedEntryStart = entryStart > dayStart ? entryStart : dayStart;
                            var clippedEntryEnd = entryEnd < dayEnd ? entryEnd : dayEnd;
                            return new
                            {
                                TimeEntry = te,
                                Start = clippedEntryStart,
                                End = clippedEntryEnd
                            };
                        })
                        .Where(te => te.Start < te.End)
                        .OrderBy(te => te.Start)
                        .ThenBy(te => te.End)
                        .Select(te => new ReportShiftTimeEntryDto(
                            te.TimeEntry.Id,
                            te.TimeEntry.OrganizationMemberId,
                            memberNameByOrganizationMemberId.GetValueOrDefault(te.TimeEntry.OrganizationMemberId, "Unknown"),
                            TimeOnly.FromDateTime(te.Start.UtcDateTime),
                            te.TimeEntry.ClockOutAt.HasValue ? TimeOnly.FromDateTime(te.End.UtcDateTime) : null,
                            te.TimeEntry.Status))
                        .ToList()))
                .ToList();

            points.Add(new ReportDailyActivityDto(
                date,
                taskMap.GetValueOrDefault(date),
                timeEntryMap.GetValueOrDefault(date),
                submissionMap.GetValueOrDefault(date),
                shiftAssignments));
        }

        return points;
    }

    private static IReadOnlyCollection<ReportSignalDto> BuildSignals(ReportOverviewDto overview)
    {
        var signals = new List<ReportSignalDto>
        {
            new("open_tasks", "Open tasks", overview.OpenTasksCount.ToString()),
            new("overdue_tasks", "Overdue tasks", overview.OverdueTasksCount.ToString(), overview.OverdueTasksCount > 0 ? "danger" : "default"),
            new("active_shifts", "Active shifts now", overview.ActiveShiftsNow.ToString()),
            new("missing_forms", "Shifts missing forms", overview.MissingRequiredShiftForms.ToString(), overview.MissingRequiredShiftForms > 0 ? "danger" : "default"),
            new("pending_leaves", "Pending leave requests", overview.PendingLeaveRequests.ToString(), overview.PendingLeaveRequests > 0 ? "warning" : "default")
        };

        return signals;
    }
}