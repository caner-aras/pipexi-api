using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;
using TimeZoneConverter;

namespace Pipexi.Application.Features.Reports.Queries.GetShiftReportData;

public sealed record GetShiftReportDataQuery(
    Guid OrganizationId, 
    DateTime FromDate, 
    DateTime ToDate, 
    Guid[]? MemberIds = null, 
    bool IncludeSummary = false)
    : IQuery<Result<ShiftReportDataResponseDto>>;

public sealed record ShiftReportDataResponseDto(
    string Currency,
    string Timezone,
    List<ShiftReportItemDto> Shifts,
    List<ShiftReportSummaryDto> Summary);

public sealed record ShiftReportItemDto(
    Guid ShiftId,
    string EmployeeName,
    DateTime Date,
    DateTime ScheduledStartAt,
    DateTime ScheduledEndAt,
    double ScheduledHours,
    double ActualHours,
    double Overtime,
    decimal Earnings,
    List<ShiftReportEntryDto> Entries);

public sealed record ShiftReportEntryDto(
    DateTime? In,
    DateTime? Out,
    string Action); // "Entry", "Break (Paid)", "Break (Unpaid)"

public sealed record ShiftReportSummaryDto(
    DateTime Date,
    double ScheduledHours,
    double ActualHours,
    decimal Earnings);

public sealed class Handler : IRequestHandler<GetShiftReportDataQuery, Result<ShiftReportDataResponseDto>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
    private readonly IMemberPositionHistoryRepository _memberPositionHistoryRepository;

    public Handler(
        IOrganizationRepository organizationRepository,
        IShiftRepository shiftRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        ITimeEntryRepository timeEntryRepository,
        ITimeEntryBreakRepository timeEntryBreakRepository,
        IMemberPositionHistoryRepository memberPositionHistoryRepository)
    {
        _organizationRepository = organizationRepository;
        _shiftRepository = shiftRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _userRepository = userRepository;
        _timeEntryRepository = timeEntryRepository;
        _timeEntryBreakRepository = timeEntryBreakRepository;
        _memberPositionHistoryRepository = memberPositionHistoryRepository;
    }

    public async Task<Result<ShiftReportDataResponseDto>> Handle(GetShiftReportDataQuery request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
        if (organization is null)
        {
            return Result<ShiftReportDataResponseDto>.Failure(
                new AppError("general.not_found", $"Organization {request.OrganizationId} not found."),
                (int)HttpStatusCode.NotFound);
        }

        TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo(organization.Timezone);

        var allShifts = await _shiftRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        
        var filteredShifts = allShifts
            .Where(s => s.StartAt.Date >= request.FromDate.Date && s.StartAt.Date <= request.ToDate.Date)
            .ToList();

        if (request.MemberIds != null && request.MemberIds.Length > 0)
        {
            filteredShifts = filteredShifts.Where(s => s.OrganizationMemberId.HasValue && request.MemberIds.Contains(s.OrganizationMemberId.Value)).ToList();
        }

        var shiftIds = filteredShifts.Select(s => s.Id).ToList();
        var timeEntries = shiftIds.Count > 0 
            ? await _timeEntryRepository.ListByShiftIdsAsync(shiftIds, cancellationToken)
            : new List<TimeEntry>();
            
        filteredShifts = filteredShifts
            .Where(s => timeEntries.Any(te => te.ShiftId == s.Id))
            .OrderBy(s => s.StartAt)
            .ToList();

        var timeEntryIds = timeEntries.Select(te => te.Id).ToList();
        var breaks = timeEntryIds.Count > 0
            ? await _timeEntryBreakRepository.ListByTimeEntryIdsAsync(timeEntryIds, cancellationToken)
            : new List<TimeEntryBreak>();

        var members = await _organizationMemberRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var userIds = members.Select(m => m.UserId).Distinct().ToList();
        var users = new Dictionary<Guid, User>();
        foreach(var userId in userIds)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null) users[userId] = user;
        }

        var memberNames = members.ToDictionary(
            m => m.Id,
            m => users.TryGetValue(m.UserId, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "Unknown");

        var memberWages = new Dictionary<Guid, decimal>();
        foreach (var memberId in members.Select(m => m.Id))
        {
            var activeHistory = await _memberPositionHistoryRepository.GetActiveByOrganizationMemberIdAsync(memberId, cancellationToken);
            memberWages[memberId] = activeHistory?.HourlyRate ?? 0m;
        }

        var shiftItems = new List<ShiftReportItemDto>();
        
        foreach (var shift in filteredShifts)
        {
            var employeeName = shift.OrganizationMemberId.HasValue && memberNames.TryGetValue(shift.OrganizationMemberId.Value, out var name) 
                ? name 
                : "Unassigned";

            var hourlyRate = shift.OrganizationMemberId.HasValue && memberWages.TryGetValue(shift.OrganizationMemberId.Value, out var wage)
                ? wage
                : 0m;

            var shiftEntries = timeEntries.Where(te => te.ShiftId == shift.Id).OrderBy(te => te.ClockInAt).ToList();
            var scheduledDuration = (shift.EndAt - shift.StartAt).TotalHours;
            double totalActualDuration = 0;
            var entryDtos = new List<ShiftReportEntryDto>();

            foreach (var entry in shiftEntries)
            {
                var entryBreaks = breaks.Where(b => b.TimeEntryId == entry.Id).OrderBy(b => b.StartAt).ToList();
                var clockOutTime = entry.ClockOutAt ?? shift.EndAt;
                var entryDuration = (clockOutTime - entry.ClockInAt).TotalHours;
                var unpaidBreaksDuration = entryBreaks.Where(b => !b.IsPaid).Sum(b => (b.EndAt - b.StartAt).TotalHours);
                var actualEntryDuration = Math.Max(0, entryDuration - unpaidBreaksDuration);
                
                totalActualDuration += actualEntryDuration;

                var clockInTz = TimeZoneInfo.ConvertTimeFromUtc(entry.ClockInAt.UtcDateTime, tzi);
                var clockOutTz = TimeZoneInfo.ConvertTimeFromUtc(clockOutTime.UtcDateTime, tzi);

                entryDtos.Add(new ShiftReportEntryDto(clockInTz, clockOutTz, "Entry"));

                foreach (var b in entryBreaks)
                {
                    var breakStartTz = TimeZoneInfo.ConvertTimeFromUtc(b.StartAt.UtcDateTime, tzi);
                    var breakEndTz = TimeZoneInfo.ConvertTimeFromUtc(b.EndAt.UtcDateTime, tzi);
                    entryDtos.Add(new ShiftReportEntryDto(breakStartTz, breakEndTz, $"Break {(b.IsPaid ? "(Paid)" : "(Unpaid)")}"));
                }
            }

            var overtime = Math.Max(0, totalActualDuration - scheduledDuration);
            var totalWage = (decimal)totalActualDuration * hourlyRate;
            var shiftStartTz = TimeZoneInfo.ConvertTimeFromUtc(shift.StartAt.UtcDateTime, tzi);
            var shiftEndTz = TimeZoneInfo.ConvertTimeFromUtc(shift.EndAt.UtcDateTime, tzi);

            shiftItems.Add(new ShiftReportItemDto(
                shift.Id,
                employeeName,
                shiftStartTz.Date,
                shiftStartTz,
                shiftEndTz,
                scheduledDuration,
                totalActualDuration,
                overtime,
                totalWage,
                entryDtos
            ));
        }

        var summaryItems = new List<ShiftReportSummaryDto>();
        
        if (request.IncludeSummary && filteredShifts.Any())
        {
            var groupedShifts = filteredShifts.GroupBy(s => TimeZoneInfo.ConvertTimeFromUtc(s.StartAt.UtcDateTime, tzi).Date).OrderBy(g => g.Key).ToList();
            
            foreach (var group in groupedShifts)
            {
                double dailySched = 0;
                double dailyActual = 0;
                decimal dailyEarnings = 0;

                foreach (var shift in group)
                {
                    var hourlyRate = shift.OrganizationMemberId.HasValue && memberWages.TryGetValue(shift.OrganizationMemberId.Value, out var wage) ? wage : 0m;
                    var scheduledDuration = (shift.EndAt - shift.StartAt).TotalHours;
                    
                    var shiftEntries = timeEntries.Where(te => te.ShiftId == shift.Id).ToList();
                    double actualDuration = 0;
                    
                    foreach (var entry in shiftEntries)
                    {
                        var entryBreaks = breaks.Where(b => b.TimeEntryId == entry.Id).ToList();
                        var clockOutTime = entry.ClockOutAt ?? shift.EndAt;
                        var entryDuration = (clockOutTime - entry.ClockInAt).TotalHours;
                        var unpaidBreaksDuration = entryBreaks.Where(b => !b.IsPaid).Sum(b => (b.EndAt - b.StartAt).TotalHours);
                        actualDuration += Math.Max(0, entryDuration - unpaidBreaksDuration);
                    }

                    dailySched += scheduledDuration;
                    dailyActual += actualDuration;
                    dailyEarnings += (decimal)actualDuration * hourlyRate;
                }

                summaryItems.Add(new ShiftReportSummaryDto(
                    group.Key,
                    dailySched,
                    dailyActual,
                    dailyEarnings
                ));
            }
        }

        return Result<ShiftReportDataResponseDto>.Success(
            new ShiftReportDataResponseDto(organization.Currency, organization.Timezone, shiftItems, summaryItems));
    }
}
